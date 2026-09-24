using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;

static class Engine
{
    public const string Perfo = "dd3829b0-aec8-4c97-bb85-e770061b304a";
    public const string Saver = "60f77099-9fd1-4dff-bc41-117060d759b0";
    public static readonly string Dell = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Dell\DellOptimizer\do-cli.exe");
    public static readonly string Power = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "powercfg.exe");
    public static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "son-islem.txt");
    public static string Run(string file, string args)
    {
        using (var p = new Process())
        {
            p.StartInfo = new ProcessStartInfo(file, args) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
            p.Start();
            var stdout = p.StandardOutput.ReadToEndAsync(); var stderr = p.StandardError.ReadToEndAsync();
            if (!p.WaitForExit(45000)) { try { p.Kill(); } catch {} throw new Exception("Komut zaman aşımına uğradı; son durum kontrol edilmeli."); }
            Task.WaitAll(stdout, stderr);
            if (p.ExitCode != 0) throw new Exception((file == Dell ? "Dell Optimizer" : "Windows") + " hata " + p.ExitCode + ": " + stdout.Result + " " + stderr.Result + (p.ExitCode == 9 ? "\nDell Optimizer penceresini kapatıp tekrar deneyin." : ""));
            return stdout.Result;
        }
    }
    public static string Active() { return Regex.Match(Run(Power, "/getactivescheme"), @"[0-9a-fA-F]{8}-(?:[0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}").Value.ToLowerInvariant(); }
    public static string GetDell(string setting) { return Run(Dell, "/get -name=SystemPowerConfiguration." + setting); }
    public static void SetDell(string setting, string value) { Run(Dell, "/configure -name=SystemPowerConfiguration." + setting + " -value=" + value); }
    public static string Value(string output)
    {
        // Dell CLI output is either JSON or a labeled list. Match only the current value, never the list of allowed values.
        var m = Regex.Match(output, "(?im)^\\s*\"?(?:Current\\s*Value|CurrentValue|Value)\"?\\s*[:=]\\s*\"?([^\"\\r\\n,}]+)");
        if (!m.Success) throw new Exception("Dell'in mevcut değeri okunamadı. Ayrıntılar: " + output);
        return m.Groups[1].Value.Trim();
    }
    [StructLayout(LayoutKind.Sequential)] public struct PowerStatus { public byte AC, Battery, Percent, Saver; public uint Life, Full; }
    [DllImport("kernel32.dll")] static extern bool GetSystemPowerStatus(out PowerStatus s);
    public static bool EnergyOn() { PowerStatus s; if (!GetSystemPowerStatus(out s)) throw new Exception("Energy saver durumu okunamadı."); return s.Saver == 1; }

    [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern IntPtr FindWindow(string cls,string title);
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hwnd,out uint pid);
    static AutomationElement EnergySettingsWindow()
    {
        foreach(string title in new[]{"Settings","Ayarlar"}) {
            var h=FindWindow(null,title);if(h==IntPtr.Zero)continue;
            uint pid;GetWindowThreadProcessId(h,out pid);
            using(var p=Process.GetProcessById((int)pid)) if(p.ProcessName!="ApplicationFrameHost" && p.ProcessName!="SystemSettings")continue;
            SetForegroundWindow(h);return AutomationElement.FromHandle(h);
        }
        return null;
    }
    static AutomationElement EnergyControl(bool navigate)
    {
        const string id="SystemSettings_PowerAndBattery_EnergySaverAlwaysOn_ToggleSwitch";
        var existing=EnergySettingsWindow();
        if(existing!=null){var control=existing.FindFirst(TreeScope.Descendants,new PropertyCondition(AutomationElement.AutomationIdProperty,id));if(control!=null)return control;}
        if(!navigate)return null;
        dynamic windows=Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("9BA05972-F6A8-11CF-A442-00A0C90A8F39")));
        object location=0,rootLocation=0;int desktopHwnd;
        dynamic desktop=windows.FindWindowSW(ref location,ref rootLocation,8,out desktopHwnd,1);
        // This URI opens the Energy saver section itself; powersleep may stop at
        // the generic Power & battery page on this Windows build.
        desktop.Document.Application.ShellExecute("ms-settings:batterysaver","","","open",1);
        bool expanded=false;
        for(int attempt=0;attempt<15;attempt++) {
            Thread.Sleep(250);
            var root=EnergySettingsWindow();if(root==null)continue;
            var control=root.FindFirst(TreeScope.Descendants,new PropertyCondition(AutomationElement.AutomationIdProperty,id));if(control!=null)return control;
            if(expanded)continue;
            // Scope the expander to the Energy saver group itself, NEVER to an
            // ancestor page. Other sections reuse the same EntityItemButton ID.
            foreach(AutomationElement group in root.FindAll(TreeScope.Descendants,new PropertyCondition(AutomationElement.ControlTypeProperty,ControlType.Group))) {
                if(!Regex.IsMatch(group.Current.Name,@"^(Energy saver|Enerji tasarrufu)(,|$)",RegexOptions.IgnoreCase))continue;
                object pattern;
                if(group.TryGetCurrentPattern(ExpandCollapsePattern.Pattern,out pattern)){((ExpandCollapsePattern)pattern).Expand();expanded=true;break;}
                var button=group.FindFirst(TreeScope.Children,new PropertyCondition(AutomationElement.AutomationIdProperty,"EntityItemButton"));
                if(button==null)button=group.FindFirst(TreeScope.Descendants,new PropertyCondition(AutomationElement.AutomationIdProperty,"EntityItemButton"));
                if(button!=null && button.TryGetCurrentPattern(InvokePattern.Pattern,out pattern)){((InvokePattern)pattern).Invoke();expanded=true;break;}
            }
        }
        throw new Exception("Always use energy saver anahtarı bulunamadı. Ayarlar > Sistem > Güç ve pil > Energy saver bölümünü açıp yeniden deneyin.");
    }
    public static string InspectEnergy(bool navigate)
    {
        var control=EnergyControl(navigate);if(control==null)return "Energy saver anahtarı: sayfa kapalı.";
        return "Düğme: "+control.Current.Name+"\r\nAutomationId: "+control.Current.AutomationId+"\r\nAnahtar: "+((TogglePattern)control.GetCurrentPattern(TogglePattern.Pattern)).Current.ToggleState+"\r\nWindows Energy saver: "+EnergyOn();
    }
    public static void Energy(bool on)
    {
        var control=EnergyControl(true);var toggle=(TogglePattern)control.GetCurrentPattern(TogglePattern.Pattern);
        if(!control.Current.IsEnabled)throw new Exception("Windows Energy saver anahtarı devre dışı.");
        if((toggle.Current.ToggleState==ToggleState.On)!=on)toggle.Toggle();
        for(int i=0;i<20;i++){Thread.Sleep(250);if((toggle.Current.ToggleState==ToggleState.On)==on && EnergyOn()==on)return;}
        throw new Exception("Energy saver değişikliği doğrulanamadı. Sağ alttaki düğmeyi kontrol edin.");
    }
    public static void SetEnergyFromMain(bool on)
    {
        // Windows Settings is intentionally kept at the user's normal integrity
        // level. The elevated Dell/power-plan app therefore delegates only this
        // one UI action to a same-user helper.
        var helper = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"PerfoSaver-EnergyHelper.exe");
        if(!File.Exists(helper)) throw new Exception("Energy saver yardımcı uygulaması bulunamadı.");
        var result = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"energy-helper-result.txt");
        try { if(File.Exists(result)) File.Delete(result); } catch {}
        dynamic windows=Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("9BA05972-F6A8-11CF-A442-00A0C90A8F39")));
        object location=0,rootLocation=0;int desktopHwnd;
        dynamic desktop=windows.FindWindowSW(ref location,ref rootLocation,8,out desktopHwnd,1);
        desktop.Document.Application.ShellExecute(helper,on ? "--helper-on" : "--helper-off",AppDomain.CurrentDomain.BaseDirectory,"open",1);
        for(int i=0;i<40;i++) {
            Thread.Sleep(150);
            if(!File.Exists(result)) continue;
            var text=File.ReadAllText(result);
            if(text.StartsWith("OK")) { if(EnergyOn()==on)return; throw new Exception("Energy saver durumu doğrulanamadı."); }
            throw new Exception(text);
        }
        throw new Exception("Energy saver yardımcı uygulaması zamanında yanıt vermedi.");
    }
    public static string Diagnose()
    {
        var b=new StringBuilder();b.AppendLine("Plan: "+Active());b.AppendLine("Energy saver: "+EnergyOn());
        foreach(var setting in new[]{"ThermalMode","SyncThermalModeAndWindowsPowerSlider"})try{b.AppendLine(setting+":\n"+GetDell(setting));}catch(Exception e){b.AppendLine(e.Message);}
        b.AppendLine(InspectEnergy(false));return b.ToString();
    }
    public static void SelfTest()
    {
        var path=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"test-sonucu.txt");
        var b=new StringBuilder(); string plan=null,thermal=null,sync=null; bool energy=false;
        Action save=()=>File.WriteAllText(path,b.ToString(),Encoding.UTF8);
        try {
            plan=Active();thermal=Value(GetDell("ThermalMode"));sync=Value(GetDell("SyncThermalModeAndWindowsPowerSlider"));energy=EnergyOn();
            b.AppendLine("Başlangıç: "+plan+", Dell="+thermal+", Eşitleme="+sync+", Energy="+energy);save();
            foreach(bool perfo in new[]{false,true}) {b.AppendLine(perfo?"PERFO testi":"SAVER testi");save();Apply(perfo,(i,t)=>{b.AppendLine(t);save();});b.AppendLine("GEÇTİ");save();}
        }catch(Exception e){b.AppendLine("BAŞARISIZ: "+e);save();}
        finally {
            if(thermal!=null)try{SetDell("ThermalMode",thermal);b.AppendLine("Dell başlangıç değerine döndü.");}catch(Exception e){b.AppendLine("GERİ YÜKLEME HATASI: "+e.Message);}
            if(sync!=null)try{SetDell("SyncThermalModeAndWindowsPowerSlider",sync);}catch(Exception e){b.AppendLine("GERİ YÜKLEME HATASI: "+e.Message);}
            if(plan!=null)try{Run(Power,"/setactive "+plan);Energy(energy);b.AppendLine("Başlangıç ayarları geri yüklendi. Plan="+Active()+", Dell="+Value(GetDell("ThermalMode"))+", Eşitleme="+Value(GetDell("SyncThermalModeAndWindowsPowerSlider"))+", Energy="+EnergyOn());}catch(Exception e){b.AppendLine("GERİ YÜKLEME HATASI: "+e.Message);}
            save();
        }
    }
    public static void Apply(bool perfo, Action<int,string> report)
    {
        var log = new StringBuilder(DateTime.Now.ToString("s") + "\n" + (perfo ? "PERFO" : "SAVER") + "\n");
        int step = 0;
        try {
            if (!File.Exists(Dell)) throw new Exception("Dell Optimizer komut aracı bulunamadı.");
            var id = perfo ? Perfo : Saver;
            if (!Run(Power, "/list").Contains(id)) throw new Exception("Bu bilgisayar için kayıtlı güç planı bulunamadı.");
            var previous = Active();
            var thermal = Value(GetDell("ThermalMode"));
            var sync = Value(GetDell("SyncThermalModeAndWindowsPowerSlider"));
            log.AppendLine("Önceki plan: " + previous + "\nÖnceki Dell: " + thermal + "\nÖnceki eşitleme: " + sync + "\nÖnceki Energy saver: " + EnergyOn());
            File.WriteAllText(LogPath, log.ToString(), Encoding.UTF8);
            report(0,"Dell ayarlanıyor…");
            // Independent explicit presets should not be overwritten by Dell's Windows slider synchronization.
            if (!sync.Equals("False", StringComparison.OrdinalIgnoreCase)) SetDell("SyncThermalModeAndWindowsPowerSlider", "False");
            SetDell("ThermalMode", perfo ? "Ultra" : "Quiet");
            var actual = Value(GetDell("ThermalMode"));
            if (!actual.Equals(perfo ? "Ultra" : "Quiet",StringComparison.OrdinalIgnoreCase)) throw new Exception("Dell termal modu doğrulanamadı: " + actual);
            report(0,"Dell: " + (perfo ? "Ultra Performance" : "Quiet") + "  ✓"); log.AppendLine("Dell doğrulandı."); step=1;
            report(1,"Güç planı uygulanıyor…"); Run(Power, "/setactive " + id);
            if (Active()!=id) throw new Exception("Windows güç planı doğrulanamadı.");
            report(1,"Windows: " + (perfo ? "Ultimate Performance" : "Power saver") + "  ✓"); log.AppendLine("Güç planı doğrulandı."); step=2;
            report(2,"Energy saver ayarlanıyor…"); SetEnergyFromMain(!perfo);
            // Recheck after the energy switch, in case another application changed the profile meanwhile.
            if (Active()!=id || !Value(GetDell("ThermalMode")).Equals(perfo ? "Ultra" : "Quiet",StringComparison.OrdinalIgnoreCase)) throw new Exception("Başka bir uygulama güç ayarlarını değiştirdi; yeniden deneyin.");
            report(2,"Energy saver: " + (perfo ? "Kapalı" : "Açık") + "  ✓"); log.AppendLine("Energy saver doğrulandı.");
        } catch(Exception ex) { report(step,"Tamamlanamadı: " + ex.Message); log.AppendLine("HATA: " + ex); throw; }
        finally { File.WriteAllText(LogPath, log.ToString(), Encoding.UTF8); }
    }
}

class MainForm : Form
{
    Label status; Label[] rows = new Label[3]; Button perfo, saver; bool busy;
    public MainForm()
    {
        SuspendLayout();
        Text="Perfo / Saver";StartPosition=FormStartPosition.CenterScreen;FormBorderStyle=FormBorderStyle.FixedSingle;MaximizeBox=false;
        BackColor=Color.FromArgb(20,25,34);ForeColor=Color.White;Font=new Font("Segoe UI",10);
        // Fonts already use DPI-scaled points. Scale fixed spacing explicitly, and
        // let labels request their actual measured height instead of clipping them.
        AutoScaleMode=AutoScaleMode.None;
        // Windows already scales the text. Keep the dialog compact at 200% DPI.
        int width=520,pad=16,gap=6;
        ClientSize=new Size(width,360);MinimumSize=new Size(width+2*SystemInformation.FixedFrameBorderSize.Width,380);
        AutoSize=true;AutoSizeMode=AutoSizeMode.GrowAndShrink;
        var layout=new TableLayoutPanel {ColumnCount=1,RowCount=8,AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,Dock=DockStyle.Top,Padding=new Padding(pad),BackColor=BackColor};
        layout.MinimumSize=new Size(width,0);layout.MaximumSize=new Size(width,0);
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        Func<string,float,Color,Label> label=(text,size,color)=>new Label {Text=text,AutoSize=true,MaximumSize=new Size(width-2*pad,0),Font=new Font("Segoe UI",size),ForeColor=color,Margin=new Padding(0,0,0,gap)};
        layout.Controls.Add(label("Çalışma modunu seç",13,Color.White),0,0);
        layout.Controls.Add(label("CATIA için güç. Günlük kullanım için sessizlik.",8.5f,Color.FromArgb(190,202,218)),0,1);
        var choices=new TableLayoutPanel {ColumnCount=3,RowCount=1,Dock=DockStyle.Top,Height=64,Margin=new Padding(0,gap/2,0,gap)};
        choices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));choices.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,gap));choices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        perfo=MakeButton("PERFO\nCATIA · Ultra Performance",Color.FromArgb(52,112,220));
        saver=MakeButton("SAVER\nGünlük · Quiet",Color.FromArgb(29,120,92));
        choices.Controls.Add(perfo,0,0);choices.Controls.Add(saver,2,0);layout.Controls.Add(choices,0,2);
        perfo.Click+=async delegate {await Apply(true);};saver.Click+=async delegate {await Apply(false);};
        for(int i=0;i<3;i++) {rows[i]=label(new[]{"Dell","Windows","Energy saver"}[i]+": —",8.5f,Color.FromArgb(220,228,238));layout.Controls.Add(rows[i],0,3+i);}
        status=label("Bir mod seçin; üç ayar birlikte uygulanır.",8.5f,Color.FromArgb(190,202,218));status.Margin=new Padding(0,gap,0,gap);layout.Controls.Add(status,0,6);
        var footer=new FlowLayoutPanel {AutoSize=true,Dock=DockStyle.Top,Margin=Padding.Empty};
        var details=new LinkLabel {Text="Son işlem",AutoSize=true,LinkColor=Color.LightSteelBlue,Margin=new Padding(0,0,gap*2,0)};details.LinkClicked+=delegate {if(File.Exists(Engine.LogPath))Process.Start("notepad.exe","\""+Engine.LogPath+"\"");};footer.Controls.Add(details);
        var check=new LinkLabel {Text="Tanılama",AutoSize=true,LinkColor=Color.LightSteelBlue};check.LinkClicked+=async delegate {if(busy)return;busy=true;try{string text=await Task.Run(()=>Engine.Diagnose());File.WriteAllText(Engine.LogPath,text,Encoding.UTF8);Process.Start("notepad.exe","\""+Engine.LogPath+"\"");}catch(Exception e){MessageBox.Show(e.Message);}finally{busy=false;}};footer.Controls.Add(check);
        layout.Controls.Add(footer,0,7);Controls.Add(layout);FormClosing+=delegate(object s,FormClosingEventArgs e){if(busy)e.Cancel=true;};
        ResumeLayout(true);
    }
    Button MakeButton(string text,Color color)
    {
        var b=new Button {Text=text,Dock=DockStyle.Fill,Margin=Padding.Empty,FlatStyle=FlatStyle.Flat,BackColor=color,ForeColor=Color.White,Font=new Font("Segoe UI Semibold",8.5f),Cursor=Cursors.Hand,UseVisualStyleBackColor=false};
        b.FlatAppearance.BorderSize=0;return b;
    }
    async Task Apply(bool p)
    {
        if(busy)return;busy=true;perfo.Enabled=saver.Enabled=false;status.Text="Uygulanıyor…";
        try {await Task.Run(()=>Engine.Apply(p,(i,text)=>BeginInvoke((Action)(()=>rows[i].Text=text.StartsWith("Tamamlanamadı:") ? new[]{"Dell","Windows güç planı","Energy saver"}[i]+": tamamlanamadı; ayrıntılar Son işlem kaydında." : text)))); status.Text=(p?"PERFO":"SAVER")+" hazır — üç ayar doğrulandı."; Activate();}
        catch(Exception e){status.Text="İşlem eksik; Son işlem bağlantısına bakın.";MessageBox.Show(this,e.Message+"\n\nTamamlanan ayarlar uygulanmış olabilir. Yeniden deneyin; ayrıntılar Son işlem kaydında.","Mod tamamen uygulanamadı",MessageBoxButtons.OK,MessageBoxIcon.Warning);}
        finally{busy=false;perfo.Enabled=saver.Enabled=true;}
    }
    [STAThread] static void Main(string[] args)
    {
        Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
        if(args.Length>0 && args[0]=="--apply-saver") {Engine.Apply(false,(i,text)=>{});return;}
        if(args.Length>0 && args[0]=="--apply-perfo") {Engine.Apply(true,(i,text)=>{});return;}
        if(args.Length>0 && (args[0]=="--helper-on" || args[0]=="--helper-off")) {var result=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"energy-helper-result.txt");try{Engine.Energy(args[0]=="--helper-on");File.WriteAllText(result,"OK",Encoding.UTF8);}catch(Exception e){File.WriteAllText(result,e.Message,Encoding.UTF8);}return;}
        if(args.Length>0 && args[0]=="--inspect-energy") {try{File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"quick-inspect.txt"),Engine.InspectEnergy(true),Encoding.UTF8);}catch(Exception e){File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"quick-inspect.txt"),e.ToString());}return;}
        if(args.Length>0 && args[0]=="--inspect-energy-current") {File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"quick-inspect.txt"),Engine.InspectEnergy(false),Encoding.UTF8);return;}
        if(args.Length>0 && args[0]=="--preview") {using(var form=new MainForm()){form.Show();Application.DoEvents();using(var bmp=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bmp,new Rectangle(0,0,form.Width,form.Height));bmp.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"onizleme.png"));}}return;}
        if(args.Length>0 && args[0]=="--selftest") {Task.Run(()=>Engine.SelfTest()).GetAwaiter().GetResult();return;}
        if(args.Length>0 && args[0]=="--energytest") {var result=new StringBuilder();bool old=Engine.EnergyOn();try{Engine.Energy(!old);result.AppendLine("ENERGY ON/OFF GEÇTİ: "+Engine.EnergyOn());}catch(Exception e){result.AppendLine(e.ToString());}finally{try{Engine.Energy(old);result.AppendLine("GERİ YÜKLENDİ: "+Engine.EnergyOn());}catch(Exception e){result.AppendLine(e.ToString());}File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"energy-test.txt"),result.ToString(),Encoding.UTF8);}return;}
        if(args.Length>0 && args[0]=="--diagnose") {try{File.WriteAllText(Engine.LogPath,Engine.Diagnose(),Encoding.UTF8);}catch(Exception e){File.WriteAllText(Engine.LogPath,e.ToString());}return;}
        bool created;using(var mutex=new Mutex(true,"Local\\PerfoSaver-UI",out created)) {
            if(!created){MessageBox.Show("Perfo / Saver zaten açık. Mevcut pencereyi kullanın.");return;}
            Application.Run(new MainForm());
        }
    }
}












//------------------------------------------------------------------------------

//  <auto-generated>
//      This code was generated by:
//        TerminalGuiDesigner v1.1.0.0
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// -----------------------------------------------------------------------------
namespace EwApiConsole {
    using System;
    using Terminal.Gui;
    
    
    public partial class MyView : Terminal.Gui.Window {
        
        private Terminal.Gui.ColorScheme greyOnBlack;
        
        private Terminal.Gui.ColorScheme blueOnBlack;
        
        private Terminal.Gui.ColorScheme tgDefault;
        
        private Terminal.Gui.ColorScheme greenOnBlack;
        
        private Terminal.Gui.ColorScheme redOnBlack;
        
        private Terminal.Gui.Label label5;
        
        private Terminal.Gui.Label label6;
        
        private Terminal.Gui.FrameView frameView;
        
        private Terminal.Gui.Label label;
        
        private Terminal.Gui.Label label2;
        
        private Terminal.Gui.Label label3;
        
        private Terminal.Gui.LineView lineView2;
        
        private Terminal.Gui.TextView textView;
        
        private Terminal.Gui.Wizard tokenInputWizard;
        
        private Terminal.Gui.TextField mondayTokenTextField;
        
        private Terminal.Gui.View view2;
        
        private Terminal.Gui.Label label7;
        
        private Terminal.Gui.LineView lineView;
        
        private Terminal.Gui.View view;
        
        private Terminal.Gui.Label label4;
        
        private void InitializeComponent() {
            this.label4 = new Terminal.Gui.Label();
            this.view = new Terminal.Gui.View();
            this.lineView = new Terminal.Gui.LineView();
            this.label7 = new Terminal.Gui.Label();
            this.view2 = new Terminal.Gui.View();
            this.mondayTokenTextField = new Terminal.Gui.TextField();
            this.tokenInputWizard = new Terminal.Gui.Wizard();
            this.textView = new Terminal.Gui.TextView();
            this.lineView2 = new Terminal.Gui.LineView();
            this.label3 = new Terminal.Gui.Label();
            this.label2 = new Terminal.Gui.Label();
            this.label = new Terminal.Gui.Label();
            this.frameView = new Terminal.Gui.FrameView();
            this.label6 = new Terminal.Gui.Label();
            this.label5 = new Terminal.Gui.Label();
            this.greyOnBlack = new Terminal.Gui.ColorScheme();
            this.greyOnBlack.Normal = new Terminal.Gui.Attribute(Terminal.Gui.Color.DarkGray, Terminal.Gui.Color.Black);
            this.greyOnBlack.HotNormal = new Terminal.Gui.Attribute(Terminal.Gui.Color.DarkGray, Terminal.Gui.Color.Black);
            this.greyOnBlack.Focus = new Terminal.Gui.Attribute(Terminal.Gui.Color.Black, Terminal.Gui.Color.DarkGray);
            this.greyOnBlack.HotFocus = new Terminal.Gui.Attribute(Terminal.Gui.Color.Black, Terminal.Gui.Color.DarkGray);
            this.greyOnBlack.Disabled = new Terminal.Gui.Attribute(Terminal.Gui.Color.DarkGray, Terminal.Gui.Color.Black);
            this.blueOnBlack = new Terminal.Gui.ColorScheme();
            this.blueOnBlack.Normal = new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightBlue, Terminal.Gui.Color.Black);
            this.blueOnBlack.HotNormal = new Terminal.Gui.Attribute(Terminal.Gui.Color.Cyan, Terminal.Gui.Color.Black);
            this.blueOnBlack.Focus = new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightBlue, Terminal.Gui.Color.BrightYellow);
            this.blueOnBlack.HotFocus = new Terminal.Gui.Attribute(Terminal.Gui.Color.Cyan, Terminal.Gui.Color.BrightYellow);
            this.blueOnBlack.Disabled = new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, Terminal.Gui.Color.Black);
            this.tgDefault = new Terminal.Gui.ColorScheme();
            this.tgDefault.Normal = new Terminal.Gui.Attribute(Terminal.Gui.Color.White, Terminal.Gui.Color.Blue);
            this.tgDefault.HotNormal = new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightCyan, Terminal.Gui.Color.Blue);
            this.tgDefault.Focus = new Terminal.Gui.Attribute(Terminal.Gui.Color.Black, Terminal.Gui.Color.Gray);
            this.tgDefault.HotFocus = new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightBlue, Terminal.Gui.Color.Gray);
            this.tgDefault.Disabled = new Terminal.Gui.Attribute(Terminal.Gui.Color.Brown, Terminal.Gui.Color.Blue);
            this.greenOnBlack = new Terminal.Gui.ColorScheme();
            this.greenOnBlack.Normal = new Terminal.Gui.Attribute(Terminal.Gui.Color.Green, Terminal.Gui.Color.Black);
            this.greenOnBlack.HotNormal = new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightGreen, Terminal.Gui.Color.Black);
            this.greenOnBlack.Focus = new Terminal.Gui.Attribute(Terminal.Gui.Color.Green, Terminal.Gui.Color.Magenta);
            this.greenOnBlack.HotFocus = new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightGreen, Terminal.Gui.Color.Magenta);
            this.greenOnBlack.Disabled = new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, Terminal.Gui.Color.Black);
            this.redOnBlack = new Terminal.Gui.ColorScheme();
            this.redOnBlack.Normal = new Terminal.Gui.Attribute(Terminal.Gui.Color.Red, Terminal.Gui.Color.Black);
            this.redOnBlack.HotNormal = new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightRed, Terminal.Gui.Color.Black);
            this.redOnBlack.Focus = new Terminal.Gui.Attribute(Terminal.Gui.Color.Red, Terminal.Gui.Color.Brown);
            this.redOnBlack.HotFocus = new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightRed, Terminal.Gui.Color.Brown);
            this.redOnBlack.Disabled = new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, Terminal.Gui.Color.Black);
            this.Width = Dim.Fill(0);
            this.Height = Dim.Fill(0);
            this.X = 0;
            this.Y = 0;
            this.Visible = true;
            this.Modal = false;
            this.IsMdiContainer = false;
            this.Border.BorderStyle = Terminal.Gui.BorderStyle.Rounded;
            this.Border.Effect3D = false;
            this.Border.Effect3DBrush = null;
            this.Border.DrawMarginFrame = true;
            this.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.Title = "";
            this.label5.Width = 4;
            this.label5.Height = 1;
            this.label5.X = Pos.Center();
            this.label5.Y = 0;
            this.label5.Visible = true;
            this.label5.Data = "label5";
            this.label5.Text = "Everyware Admnin Console";
            this.label5.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.Add(this.label5);
            this.label6.Width = 4;
            this.label6.Height = 1;
            this.label6.X = Pos.Center();
            this.label6.Y = 1;
            this.label6.Visible = true;
            this.label6.Data = "label6";
            this.label6.Text = "Michael Yung (michael.yung@everyware.com.hk)";
            this.label6.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.Add(this.label6);
            this.frameView.Width = 38;
            this.frameView.Height = 60;
            this.frameView.X = -1;
            this.frameView.Y = 2;
            this.frameView.Visible = true;
            this.frameView.Data = "frameView";
            this.frameView.Border.BorderStyle = Terminal.Gui.BorderStyle.Single;
            this.frameView.Border.Effect3D = false;
            this.frameView.Border.Effect3DBrush = null;
            this.frameView.Border.DrawMarginFrame = true;
            this.frameView.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.frameView.Title = "Function Menu";
            this.Add(this.frameView);
            this.label.Width = Dim.Fill(4);
            this.label.Height = 1;
            this.label.X = 2;
            this.label.Y = 1;
            this.label.Visible = true;
            this.label.Data = "label";
            this.label.Text = "1. Fix Shop Workday Detail";
            this.label.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.frameView.Add(this.label);
            this.label2.Width = 10;
            this.label2.Height = 5;
            this.label2.X = 2;
            this.label2.Y = 3;
            this.label2.Visible = true;
            this.label2.Data = "label2";
            this.label2.Text = "2. Fix Shop Workday \nPeriod Detail";
            this.label2.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.frameView.Add(this.label2);
            this.label3.Width = Dim.Fill(4);
            this.label3.Height = 1;
            this.label3.X = 2;
            this.label3.Y = 13;
            this.label3.Visible = true;
            this.label3.Data = "label3";
            this.label3.Text = "3. Fix Tx Payment Method";
            this.label3.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.frameView.Add(this.label3);
            this.lineView2.Width = Dim.Fill(0);
            this.lineView2.Height = 1;
            this.lineView2.X = 0;
            this.lineView2.Y = 2;
            this.lineView2.Visible = true;
            this.lineView2.Data = "lineView2";
            this.lineView2.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.lineView2.LineRune = '─';
            this.lineView2.Orientation = Terminal.Gui.Graphs.Orientation.Horizontal;
            this.Add(this.lineView2);
            this.textView.Width = 20;
            this.textView.Height = 21;
            this.textView.X = 143;
            this.textView.Y = 14;
            this.textView.Visible = true;
            this.textView.AllowsTab = true;
            this.textView.AllowsReturn = true;
            this.textView.WordWrap = true;
            this.textView.Data = "textView";
            this.textView.Text = "this is a long long text to test the text wrapping";
            this.textView.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.Add(this.textView);
            this.tokenInputWizard.Width = 40;
            this.tokenInputWizard.Height = 15;
            this.tokenInputWizard.X = Pos.Center();
            this.tokenInputWizard.Y = Pos.Center();
            this.tokenInputWizard.Visible = false;
            this.tokenInputWizard.Modal = true;
            this.tokenInputWizard.IsMdiContainer = false;
            this.tokenInputWizard.Data = "tokenInputWizard";
            this.tokenInputWizard.Text = "Welcome to Everyware Admin Console! As this is the first time you use this consol" +
                "e, please input the Monday API key. You can find the API key in the Developer Se" +
                "tting Page of Monday.h";
            this.tokenInputWizard.Border.BorderStyle = Terminal.Gui.BorderStyle.Rounded;
            this.tokenInputWizard.Border.Effect3D = true;
            this.tokenInputWizard.Border.Effect3DBrush = null;
            this.tokenInputWizard.Border.DrawMarginFrame = true;
            this.tokenInputWizard.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.tokenInputWizard.Title = "Setup";
            this.Add(this.tokenInputWizard);
            this.mondayTokenTextField.Width = 35;
            this.mondayTokenTextField.Height = 10;
            this.mondayTokenTextField.X = 1;
            this.mondayTokenTextField.Y = 7;
            this.mondayTokenTextField.Visible = true;
            this.mondayTokenTextField.ColorScheme = this.tgDefault;
            this.mondayTokenTextField.Secret = false;
            this.mondayTokenTextField.Data = "mondayTokenTextField";
            this.mondayTokenTextField.Text = "";
            this.mondayTokenTextField.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.tokenInputWizard.Add(this.mondayTokenTextField);
            this.view2.Width = 20;
            this.view2.Height = 4;
            this.view2.X = 47;
            this.view2.Y = 26;
            this.view2.Visible = true;
            this.view2.Data = "view2";
            this.view2.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.Add(this.view2);
            this.label7.Width = Dim.Fill(0);
            this.label7.Height = Dim.Fill(0);
            this.label7.X = 0;
            this.label7.Y = 0;
            this.label7.Visible = true;
            this.label7.Data = "label7";
            this.label7.Text = "Heya a label which can wrap";
            this.label7.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.view2.Add(this.label7);
            this.lineView.Width = Dim.Fill(0);
            this.lineView.Height = 1;
            this.lineView.X = 0;
            this.lineView.Y = 61;
            this.lineView.Visible = true;
            this.lineView.Data = "lineView";
            this.lineView.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.lineView.LineRune = '─';
            this.lineView.Orientation = Terminal.Gui.Graphs.Orientation.Horizontal;
            this.Add(this.lineView);
            this.view.Width = Dim.Fill(0);
            this.view.Height = 1;
            this.view.X = 0;
            this.view.Y = 62;
            this.view.Visible = true;
            this.view.ColorScheme = this.blueOnBlack;
            this.view.Data = "view";
            this.view.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.Add(this.view);
            this.label4.Width = 1;
            this.label4.Height = 1;
            this.label4.X = 0;
            this.label4.Y = 0;
            this.label4.Visible = true;
            this.label4.ColorScheme = this.tgDefault;
            this.label4.Data = "label4";
            this.label4.Text = "HeyaTi";
            this.label4.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.view.Add(this.label4);
        }
    }
}

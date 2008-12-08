using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;


namespace FileHashShell
{
    [RunInstaller(true)]
    public partial class ShellExtensionSetup : Installer
    {
        public ShellExtensionSetup()
        {
            InitializeComponent();
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void  Commit(System.Collections.IDictionary savedState)
        {
            base.Commit(savedState);
            /*
            // Get the location of regasm
            string regasmPath = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory() + @"regasm.exe";

            if (!File.Exists(regasmPath))
            {
                regasmPath = @"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\regasm.exe";
            }

            // Get the location of our DLL
            string componentPath = typeof(ShellExtensionSetup).Assembly.Location;
            // Execute regasm
            System.Diagnostics.Process.Start(regasmPath, "/codebase \"" + componentPath + "\"");*/

            Assembly asm = Assembly.GetExecutingAssembly();
            RegistrationServices reg = new RegistrationServices();
            reg.RegisterAssembly(asm, 0);
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);

            Assembly asm = Assembly.GetExecutingAssembly();
            RegistrationServices reg = new RegistrationServices();
            reg.UnregisterAssembly(asm);
        }

    }
}

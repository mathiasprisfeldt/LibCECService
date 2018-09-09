using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using System.Windows;
using Microsoft.Win32;

namespace LibCECServiceInstaller
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            InstallUtilDir.Text = Directory.GetCurrentDirectory() + @"\LibCECService.exe";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog =
                new OpenFileDialog
                {
                    Filter = "Executable (*.exe)|*.exe|All files (*.*)|*.*",
                    InitialDirectory = Directory.GetCurrentDirectory()
                };

            if (openFileDialog.ShowDialog() == true)
                InstallUtilDir.Text = openFileDialog.FileName;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            InitService();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            InitService(false);
        }

        private void InitService(bool install = true)
        {
            var serviceInstalled =
                ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "LibCECService") != null;

            if (install && serviceInstalled)
            {
                MessageBox.Show("Service is already installed.",
                    "LibCEC Service Installer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            if (!install && !serviceInstalled)
            {
                MessageBox.Show("Service is already un-installed.",
                    "LibCEC Service Installer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            var serviceExePath = InstallUtilDir.Text;

            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("Not Enough Privileges, please run this program as administrator.",
                    "LibCEC Service Installer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            if (!serviceExePath.EndsWith(".exe") || !File.Exists(serviceExePath))
            {
                MessageBox.Show("Couldn't find executable, re-check path.",
                    "LibCEC Service Installer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            var proc = new Process
            {
                StartInfo =
                {
                    FileName = serviceExePath,
                    UseShellExecute = false,
                    Arguments = install ? "install" : "uninstall"
                }
            };

            proc.Start();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            RunCmd(137);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            RunCmd(145);
        }

        private void RunCmd(int cmd)
        {
            ServiceController sc = new ServiceController("LibCECService", Environment.MachineName);

            if (sc.Status != ServiceControllerStatus.Running)
            {
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(1));
            }

            try
            {
                ServiceControllerPermission scp = new ServiceControllerPermission(ServiceControllerPermissionAccess.Control, Environment.MachineName, "LibCECService");//this will grant permission to access the Service
                scp.Assert();
                sc.Refresh();
                sc.ExecuteCommand(cmd);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message,
                    "LibCEC Service Installer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }
}
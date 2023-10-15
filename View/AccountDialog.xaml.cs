using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WowLaunchApp.ViewModule;

namespace WowLaunchApp.View
{
    /// <summary>
    /// AccountDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AccountDialog : UserControl
    {
        public AccountDialog()
        {
            InitializeComponent();
            BeforeShow();
        }

        private void BeforeShow()
        {
            switch (AccountMgr.mgrType)
            {
                case 1: //修改密码
                        pwd_txt.Text = "新密码";
                        submitBtn.Content = "确认修改";
                        break;

                case 2: //解卡角色
                        op_player.Visibility = Visibility.Visible;
                        playerName.Visibility = Visibility.Visible;
                        tipMsg.Visibility = Visibility.Visible;
                        confimPwd_txt.Visibility = Visibility.Collapsed;
                        securepwd_txt.Visibility = Visibility.Collapsed;
                        confimPwd.Visibility = Visibility.Collapsed;
                        securepwd.Visibility = Visibility.Collapsed;
                        conf_pwd_row.Height = new GridLength(0);
                        securepwd_row.Height = new GridLength(0);
                        row1.Height = new GridLength(0);
                        row2.Height = new GridLength(0);
                        submitBtn.Content = "确认解卡";
                        break;
            }
        }

        private void OnSubmitBtn(object sender, RoutedEventArgs e)
        {
            switch(AccountMgr.mgrType)
            {
                case 0: //修改密码
                    if (account.Text.Length <= 3 || pwd.Password.Length <= 4 || securepwd.Text.Length <= 4)
                    {
                        Growl.Warning("账号不得小于4位,密码不得小于4位,安全码不得小于4位.");
                        return;
                    }
                    if (pwd.Password != confimPwd.Password)
                    {
                        Growl.Warning("两次密码不一致");
                        return;
                    }
                    //注册发包
                    SocketMgr.Instance.RegisterAccount(account.Text, pwd.Password, securepwd.Text);
                    break;
                case 1://修改/找回密码
                    if (pwd.Password != confimPwd.Password)
                    {
                        Growl.Warning("新旧密码不一致");
                        return;
                    }
                    //修改密码发包
                    SocketMgr.Instance.ChangePassword(account.Text, pwd.Password, securepwd.Text);
                    break;
                case 2:
                    //角色解卡
                    SocketMgr.Instance.UnLockPlayer(account.Text, pwd.Password, playerName.Text);
                    break;
            }
        }
    }
}

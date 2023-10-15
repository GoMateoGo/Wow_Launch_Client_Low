using HandyControl.Tools.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WowLaunchApp.View;

namespace WowLaunchApp.ViewModule
{
    public class AccountMgr
    {
        private static AccountMgr _instance;
        public static int mgrType;
 
        public static AccountMgr Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AccountMgr();
                }
                return _instance;
            }
        }

        // 私有的构造函数
        private AccountMgr()
        {
            mgrType = 0;
        }
    }
}

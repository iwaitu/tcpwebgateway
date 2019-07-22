using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TcpWebGateway.Services;

namespace TcpWebGateway.Tools
{
    public class PanelHelper
    {
        private SwitchListener _switchListener;
        public PanelHelper()
        {

        }

        public void SetSwitchListener(SwitchListener switchListener)
        {
            _switchListener = switchListener;
        }

        /// <summary>
        /// 设置面板背景灯
        /// </summary>
        /// <param name="panelid">面板id,如：0B,0C,0D,0E,0F</param>
        /// <param name="buttonid">按键id,如21,22,23,24,25,26</param>
        /// <param name="value">01开,00关</param>
        /// <returns></returns>
        public async Task SetBackgroudLight(string panelid,string buttonid,int value)
        {
            var cmds = new List<string>();
            cmds.Add(string.Format("{0} 06 10 {1} 00 {2}", panelid, buttonid,value));
            await _switchListener.SendCommand(cmds);
        }

        
    }

}

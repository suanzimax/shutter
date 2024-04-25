﻿using System.Diagnostics;
using EpicsSharp.ChannelAccess.Client;
using EpicsSharp.ChannelAccess.Server;
using System;
// 发布pv,创建一个 EPICS Channel Access 服务器
var server = new CAServer();
// 创建一个 double 类型的 PV，名称为 MY_PV，初始值为 0.0
var kai0 = server.CreateRecord<EpicsSharp.ChannelAccess.Server.RecordTypes.CADoubleRecord>("clapa:Shot_Sshutter:OPEN");
var guan0 = server.CreateRecord<EpicsSharp.ChannelAccess.Server.RecordTypes.CADoubleRecord>("clapa:Shot_Sshutter:CLOSE");
var restart0 = server.CreateRecord<EpicsSharp.ChannelAccess.Server.RecordTypes.CADoubleRecord>("clapa:Shot_Sshutter:RESTART");
var error0 = server.CreateRecord<EpicsSharp.ChannelAccess.Server.RecordTypes.CADoubleRecord>("clapa:Shot_Sshutter:ERROR");
var guilin0 = server.CreateRecord<EpicsSharp.ChannelAccess.Server.RecordTypes.CADoubleRecord>("clapa:Shot_Sshutter:HOMED");
var zhuangtai0 = server.CreateRecord<EpicsSharp.ChannelAccess.Server.RecordTypes.CADoubleRecord>("clapa:Shot_Sshutter:STATE");
var wait0 = server.CreateRecord<EpicsSharp.ChannelAccess.Server.RecordTypes.CADoubleRecord>("clapa:Shot_Sshutter:WIDTH");
var shootserial0 = server.CreateRecord<EpicsSharp.ChannelAccess.Server.RecordTypes.CADoubleRecord>("clapa:Shot_Sshutter:SHOOTSERIAL");
// 设置pv初值
kai0.Value = 0.0;
guan0.Value = 0.0;
restart0.Value = 0.0;
error0.Value = 0;
guilin0.Value = 0;
zhuangtai0.Value = 2;
// 启动 EPICS 服务器
server.Start();

// 初始化进程
CommandInterfaceXPS.XPS m_xps = null;
m_xps = new CommandInterfaceXPS.XPS();
// 打开XPS连接
    m_xps.OpenInstrument("192.168.1.254", 5001, 1000);
if (m_xps != null)
{
    string XPSversion = string.Empty;
    string errorString = string.Empty;
    int result = m_xps.FirmwareVersionGet(out XPSversion, out errorString);
    if (result == 0)
    {
        Console.WriteLine("连接良好");
    }
    m_xps.KillAll(out errorString);
    m_xps.GroupInitialize("Group1",out errorString);
    m_xps.GroupHomeSearch("Group1",out errorString);

}

System.Threading.Thread.Sleep(20000);
Console.WriteLine("初始化结束");
string errorString0 = string.Empty;
//关操作
m_xps.GroupMoveAbsolute("Group1", [-100], 1, out errorString0);

string errorStringt = string.Empty;
double[] bt = new double[0];
m_xps.GPIOAnalogGet(["GPIO4.ADC1"], out bt, out errorStringt);

//Console.WriteLine(kai0.GetDouble("KAI"));
var client = new CAClient();
client.Configuration.SearchAddress = "192.168.1.253:5064";
var kai = client.CreateChannel<string>("clapa:Shot_Sshutter:OPEN");
var guan = client.CreateChannel<string>("clapa:Shot_Sshutter:CLOSE");
var errort = client.CreateChannel<string>("clapa:Shot_Sshutter:ERROR");
var guilin = client.CreateChannel<string>("clapa:Shot_Sshutter:HOMED");
var zhuangtai = client.CreateChannel<string>("clapa:Shot_Sshutter:STATE");
var wait = client.CreateChannel<string>("clapa:Shot_Sshutter:WIDTH");
var  shootserial= client.CreateChannel<string>("clapa:Shot_Sshutter:SHOOTSERIAL");
guilin.Put(1); //完成归零后，值置为1
double n=0;
while (true)
{  
    Stopwatch stopwatch = new Stopwatch();
    stopwatch.Start();
    double kaivalue = double.Parse(kai.Get());
    if (kaivalue == 1)
    {
        string errorString = string.Empty;
        //开操作
        m_xps.GroupMoveAbsolute("Group1", [100], 1, out errorString);
        stopwatch.Stop();
        TimeSpan duration = stopwatch.Elapsed;
        Console.WriteLine("开操作时长: " + duration);
        
        kai.Put(0);
    }
    
    double guanvalue = double.Parse(guan.Get());

    if (guanvalue == 1)
    {
        string errorString = string.Empty;
        //关操作
        m_xps.GroupMoveAbsolute("Group1", [-100], 1, out errorString);

        stopwatch.Stop();
        TimeSpan duration = stopwatch.Elapsed;
        Console.WriteLine("关操作时长: " + duration);
        
        guan.Put(0);
    }

// 触发操作
        string errorString1 = string.Empty;
        bool error = false;
        double[] b = new double[0];
        m_xps.GPIOAnalogGet(["GPIO4.ADC1"], out b, out errorString1);
        if (!string.IsNullOrEmpty(errorString1) || b[0] > 80.0)
        {
            Console.WriteLine("out error:"+errorString1);
            Console.WriteLine(!string.IsNullOrEmpty(errorString1));
            Console.WriteLine(b[0] > 80.0);
            Console.WriteLine("total logistic: "+(!string.IsNullOrEmpty(errorString1) || b[0] > 80.0));
            Console.WriteLine("out b:"+b[0]);

            Stopwatch totalStopwatch = new Stopwatch();
            stopwatch.Start();
                    do
                    {
                        m_xps.GPIOAnalogGet(["GPIO4.ADC1"], out b, out errorString1);
                        if(!string.IsNullOrEmpty(errorString1) || b[0] > 80.0)
                        {
                            Console.WriteLine("inter error:"+errorString1);
                            Console.WriteLine("inter b:"+b[0]);
                            error=true;
                        }
                        else
                        {
                            error = false;
                        }
                    }
                    while(error);
            totalStopwatch.Stop();
            TimeSpan duration = stopwatch.Elapsed;
            
            
            //Console.WriteLine("total time:"+ duration);
        }
       
      
            //Console.WriteLine("当前GPIO值: " + b[0]);
            if (b[0] > 2.0)
            {
                m_xps.GroupMoveAbsolute("Group1", [100], 1, out errorString1);
                int waitvalue = int.Parse(wait.Get());
                zhuangtai.Put(1);
                Console.WriteLine("当前GPIO值: " + b[0]);
                System.Threading.Thread.Sleep(waitvalue); //以ms为计算单位
                m_xps.GroupMoveAbsolute("Group1", [-100], 1, out errorString1);
                zhuangtai.Put(0);
                n=n+1;
                shootserial.Put(n);
                Console.WriteLine("Serial Number: " + n);
            }
        

        double[] a1 = new double[0];
        m_xps.GroupPositionCurrentGet("Group1", out a1, 1, out errorString1);
        
        foreach (double z in a1)
        {
            //Console.WriteLine("当前位置: " + z);
                if (z>-150 && z<150)
            {
                if (z>70)
                {zhuangtai.Put(1);}
                else
                {zhuangtai.Put(0);}
            }
        }
        stopwatch.Stop();
        TimeSpan duration1 = stopwatch.Elapsed;
       // Console.WriteLine("触发操作时长: " + duration1);
        
}


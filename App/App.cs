using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using NQuotes;
using static forexAI.Logger;

namespace forexAI
{
  public static class App
  {
    // MqlApi object
    public static MqlApi mqlApi = null;
    public static Version version = Assembly.GetExecutingAssembly().GetName().Version;
    public static string currentNetworkId = string.Empty;
    public static Config config = new Config();
    public static PerformanceCounter processorPerformanceCounter = null;
    public static object ordersHistoryLock = new object();

    static App()
    {
      log($"[static Core created mqlApi={mqlApi} version={version}]", "App.full");
    }
  }
}
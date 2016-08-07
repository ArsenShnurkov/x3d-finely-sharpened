using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABCTVBroadcast;


//basic struct for the broadcast data.
[System.Serializable]
public class Broadcast
{
    public string series;

    public IList<Episode> Episodes;

    public Broadcast()
    {
        Episodes = new List<Episode>();
    }

    public static string Header
    {
        get
        {
            return "Programme | Region | Date | Duration";
        }
    }
}
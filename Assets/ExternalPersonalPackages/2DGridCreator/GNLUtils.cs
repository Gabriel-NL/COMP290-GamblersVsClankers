using System;
using UnityEngine;
//usage: using static CustomUtils.GNLUtils;
namespace CustomUtils
{
    public static class GNLUtils
    {

        public static void GNLPrint(string debug_msg, bool debug_state)
        {
            if (debug_state)
            {
                Debug.Log(debug_msg);
            }
        }

        public static void GNLPrintErr(string debug_msg, bool debug_state)
        {
            if (debug_state)
            {
                Debug.LogError(debug_msg);
            }
        }
    }
}

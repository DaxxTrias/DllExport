// This code was generated by a vsSolutionBuildEvent. 
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
namespace net.r_eg.DllExport
{
    using System;

    internal readonly struct DllExportVersion
    {
        public const string S_NUM = "1.8.0";
        public const string S_REV = "0";
        public const string S_REL = "";

        public const string S_NUM_REV = S_NUM + "." + S_REV;
        public const string S_PRODUCT = S_NUM + S_REL;

        public const string B_SHA1 = "-";
        public const string B_NAME = "-";
        public const int B_REVC = 0;

        public const string DXP = ".NET DllExport";
        public const string SRC = "";
        public static readonly Version number = new(S_NUM_REV);

        internal const string S_INFO        = S_NUM_REV + "+" + B_SHA1;
        internal const string S_INFO_P      = S_PRODUCT + "+" + B_SHA1;
        internal const string S_INFO_FULL   = S_NUM_REV + S_REL + "+" + B_SHA1;
    }
}
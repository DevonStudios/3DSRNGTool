﻿using System.Linq;
using Pk3DSRNGTool.Core;
using System.Windows.Forms;
using System.Globalization;

namespace Pk3DSRNGTool
{
    public class Frame_Misc
    {
        public static bool X64;
        private static readonly string[] blinkmarks = { "-", "★", "?", "? ★", "E" };

        public int Frame { get; set; }
        public int frameused;
        public int ActualFrame => Frame + frameused;
        public int Advance => Srt?.Advance ?? 0;
        public int realtime = -1;
        public string Realtime => realtime > -1 ? FuncUtil.Convert2timestr(realtime / 60.0) : string.Empty;

        public uint Rand32 { get; set; }
        public ulong Rand64 { get; set; }
        public string CurrentSeed => st?.ToString() ?? (X64 ? Rand64.ToString("X16") : Rand32.ToString("X8"));

        public int RandN { get; set; }
        public byte Pokerus { get; set; }
        public CaptureResult Crt;
        public string Capture => Crt?.ToString() ?? string.Empty;
        public SOSResult Srt;
        public string SOS => Srt?.ToString() ?? string.Empty;
        public FPFacility frt;
        public string Facility => frt?.ToString() ?? string.Empty;
        public BTTrainer trt;
        public string Trainer => trt?.ToString() ?? string.Empty;
        public byte Blink;
        public string BlinkFlag => Blink < 5 ? blinkmarks[Blink] : Blink.ToString();
        public byte Clock => (byte)(Rand64 % 17);

        public PRNGState st;
        
        public int[] status;
        public string NPCStatus => status == null ? string.Empty : string.Join(",", status.Select(i => (i > 0 ? i - 1 : i).ToString().PadLeft(2)).ToArray());
    }

    public class Misc_Filter
    {
        public bool Capture;
        public bool SOS;
        public bool Success;
        public bool Sync;
        public bool[] Slot;
        public bool HA;
        public bool Pokerus;
        public bool Random;
        public byte CompareType;
        public int Value;
        public string CurrentSeed;
        public FPFacility FacilityFilter;
        public BTTrainer TrainerFilter;
        public MaskedTextBox BaseTime;

        public bool check(Frame_Misc f)
        {
            if (Pokerus && f.Pokerus == 0)
                return false;
            if (BaseTime.Visible && (CurrentSeed != null && BaseTime.Text != ""))
            {
                string sum = ((f.Rand32 + ulong.Parse(BaseTime.Text, NumberStyles.HexNumber)) & 0xFFFFFFFF).ToString();
                string currentSeedDec = ulong.Parse(CurrentSeed, NumberStyles.HexNumber).ToString();
                if (sum.Length >= 2 && currentSeedDec.Length >= 2
                    && sum.Substring(sum.Length - 2) == currentSeedDec.Substring(currentSeedDec.Length - 2)
                    && sum.Substring(0, 1) == currentSeedDec.Substring(0, 1))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (CurrentSeed != null && !System.Text.RegularExpressions.Regex.IsMatch(f.CurrentSeed, CurrentSeed))
                return false;
            if (Random)
            {
                switch (CompareType)
                {
                    case 0: if (f.RandN >= Value) return false; break;
                    case 1: if (f.RandN < Value) return false; break;
                    case 2: if (f.RandN != Value) return false; break;
                }
            }
            if (SOS)
            {
                if (Success && !f.Srt.Success)
                    return false;
                if (HA && !f.Srt.HA)
                    return false;
                if (Sync && !f.Srt.Sync)
                    return false;
                if (Slot.Any(n => n) && !Slot[f.Srt.Slot])
                    return false;
            }
            if (Capture && Success && !f.Crt.Gotta)
                return false;
            if (FacilityFilter?.IsDifferentFrom(f.frt) ?? false)
                return false;
            if (TrainerFilter?.IsDifferentFrom(f.trt) ?? false)
                return false;
            return true;
        }
    }
}

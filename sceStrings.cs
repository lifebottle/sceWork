using StreamFAdd;
using System;
using System.Collections.Generic;

namespace sceWork
{
    internal class sceStrings
    {
        public enum OffsetType
        {
            ShortOffset = 1,
            MediumOffset = 2,
            LargeOffset = 3,
        }

        public uint baseOffset;
        public OffsetType typeOffset;
        public uint myOffset;
        public uint offset;
        public List<byte> data;
        public sceInstruction instruction;
        public bool isDeduped = false;

        public sceStrings(uint off, uint boff)
        {
            baseOffset = boff;
            myOffset = off;
            offset = 0U;
            data = new List<byte>();
        }

        public int GetStringLen(StreamFunctionAdd sfa)
        {
            int size = 0;
            byte c;

            sfa.PositionStream = offset;
            while ((c = sfa.ReadByte()) != 0)
            {
                switch (c)
                {
                    default:
                        if (c > 0x7F)
                        {
                            sfa.ReadByte();
                            size += 2;
                        } else
                        {
                            size++;
                        }
                        break;
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 0xA:
                    case 0xB:
                    case 0xC:
                    case 0xD:
                    case 0xE:
                    case 0xF:
                        sfa.ReadInt32();
                        size += 5;
                        break;
                    case 0x17:
                    case 0x18:
                    case 0x19:
                    case 0x1A:
                    case 0x1B:
                    case 0x1C:
                    case 0x1D:
                    case 0x1E:
                    case 0x1F:
                        while(sfa.ReadByte() != 0x80)
                        {
                            size++;
                        }
                        size+=2;
                        break;

                }
            }
            return size+1;
        }

        public void ReadData(StreamFunctionAdd sfa, int size = -1)
        {
            if (size <= 0)
            {
                size = GetStringLen(sfa);
                sfa.PositionStream = offset;
                for (int index = 0; index < size; ++index)
                    data.Add(sfa.ReadByte());
                return;
            }

            sfa.PositionStream = offset;
            for (int index = 0; index < size; ++index)
                data.Add(sfa.ReadByte());
        }

        public void WriteData(StreamFunctionAdd sfa)
        {
            if (!isDeduped)
            {
                sfa.WriteBytes(data);
            }
        }
    }
}
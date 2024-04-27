using StreamFAdd;
using System;
using System.Collections.Generic;

namespace sceWork
{
    internal class sceHeader
    {
        const int SECTION_COUNT = 6;
        public uint offsetScript;
        public uint offsetStrings;
        public uint[] sectionOffsets;
        public uint sectionItemCount = 0;
        public List<sceStrings> fileStrings;
        public List<int> lineNumberList;
        public List<string> plainStringList;
        public List<int> sizes;
        public Dictionary<long, sceInstruction> instructions;
        public List<long> instructionOffsets;

        public sceHeader(StreamFunctionAdd sfa)
        {
            if (sfa.ReadAnsiStringSize(8) != "TOD1RSCE")
            {
                throw new Exception("Error #1: Bad Magic");
            }
            offsetScript = sfa.ReadUInt32();
            offsetStrings = sfa.ReadUInt32();
            sfa.ReadUInt32();
            uint frame = sfa.ReadUInt32();
            uint entry = sfa.ReadUInt32();
            for (int i = 0; i < SECTION_COUNT; i++)
            {
                sectionItemCount += sfa.ReadUInt16();
            }
            sectionItemCount += 2;

            // Don't care about the per section counts
            for (int i = 0; i < SECTION_COUNT; i++)
            {
                sfa.ReadUInt16();
            }

            sectionOffsets = new uint[sectionItemCount];
            sectionOffsets[0] = frame;
            sectionOffsets[1] = entry;
            for (int i = 2; i < sectionItemCount; i++)
            {
                sfa.ReadUInt16(); sfa.ReadUInt16();
                sectionOffsets[i] = sfa.ReadUInt32();
            }

            fileStrings = new List<sceStrings>();
            sizes = new List<int>();
            sfa.PositionStream = offsetScript;
            instructions = Parse(sfa, fileStrings);

            //order the offsets by string position
            fileStrings.Sort((s1, s2) => s1.offset.CompareTo(s2.offset));

            //Get sizes
            for (int index = 0; index < fileStrings.Count; ++index)
            {
                if (index < fileStrings.Count - 1)
                    sizes.Add((int)fileStrings[index + 1].offset - (int)fileStrings[index].offset);
                else
                    sizes.Add((int)(uint)sfa.LengthStream - (int)fileStrings[index].offset);
            }
            for (int index = 0; index < fileStrings.Count; ++index)
                fileStrings[index].ReadData(sfa, sizes[index]);
        }

        private void parse0x40(StreamFunctionAdd sfa, List<byte> data)
        {
            byte bass = sfa.ReadByte();
            data.Add(bass);

            byte n = sfa.ReadByte();
            data.Add(n);

            switch (n >> 6)
            {
                case 1:
                    data.Add(sfa.ReadByte());
                    break;
                case 2:
                    data.Add(sfa.ReadByte());
                    data.Add(sfa.ReadByte());
                    break;
                case 3:
                    data.Add(sfa.ReadByte());
                    data.Add(sfa.ReadByte());
                    data.Add(sfa.ReadByte());
                    break;
            }

            uint mask = (uint)(bass >> 2) & 7;
            if (mask >= 2 && mask <= 5)
            {
                return;
            }

            mask = (uint)(n >> 4) & 3;
            if (mask == 1)
            {
                data.Add(sfa.ReadByte());
            }
            else if (mask == 2)
            {
                data.Add(sfa.ReadByte());
                data.Add(sfa.ReadByte());
            }
            else if (mask == 3)
            {
                data.Add(sfa.ReadByte());
                data.Add(sfa.ReadByte());
                data.Add(sfa.ReadByte());
            }
        }

        private void parse0x20(StreamFunctionAdd sfa, List<byte> data)
        {
            byte n = sfa.ReadByte();
            data.Add(n);

            switch ((n >> 2) & 3)
            {
                case 1:
                    data.Add(sfa.ReadByte());
                    break;
                case 2:
                    data.Add(sfa.ReadByte());
                    data.Add(sfa.ReadByte());
                    break;
                case 3:
                    data.Add(sfa.ReadByte());
                    data.Add(sfa.ReadByte());
                    data.Add(sfa.ReadByte());
                    data.Add(sfa.ReadByte());
                    break;
            }
        }
        private void parse0x80(StreamFunctionAdd sfa, List<byte> data)
        {
            byte n = sfa.ReadByte();
            data.Add(n);

            if ((n & 0x40) == 0)
            {
                return;
            }

            n = sfa.ReadByte();
            data.Add(n);

            int mask = (n >> 3) & 7;
            if (mask == 2)
            {
                data.Add(sfa.ReadByte());
            }
            else if (mask == 3)
            {
                data.Add(sfa.ReadByte());
                data.Add(sfa.ReadByte());
            }
        }

        public Dictionary<long, sceInstruction> Parse(StreamFunctionAdd sfa, List<sceStrings> fileStrings)
        {
            var fileInstructions = new Dictionary<long, sceInstruction>();
            instructionOffsets = new List<long>();
            List<byte> temp;
            uint branchTarget;

            while (sfa.GetPosition() < this.offsetStrings - 1)
            {
                temp = new List<byte>();
                // Parse the opcode
                long position = sfa.GetPosition() - offsetScript;
                instructionOffsets.Add(position);
                byte currOpcode = sfa.ReadByte();
                switch (currOpcode)
                {
                    case 1:
                    case 2:
                    case 10:
                        temp.Add(currOpcode);
                        fileInstructions[position] = new sceInstruction(sceInstruction.sceOpcode.not_implemented, position, temp);
                        continue;
                    case 3:
                        branchTarget = sfa.ReadUInt24();
                        fileInstructions[position] = new sceInstruction(sceInstruction.sceOpcode.jmp, position, branchTarget, temp);
                        continue;
                    case 4:
                        branchTarget = sfa.ReadUInt24();
                        fileInstructions[position] = new sceInstruction(sceInstruction.sceOpcode.jzs, position, branchTarget, temp);
                        continue;
                    case 5:
                        branchTarget = sfa.ReadUInt24();
                        fileInstructions[position] = new sceInstruction(sceInstruction.sceOpcode.jnz, position, branchTarget, temp);
                        continue;
                    case 6:
                        branchTarget = sfa.ReadUInt24();
                        parse0x20(sfa, temp);
                        fileInstructions[position] = new sceInstruction(sceInstruction.sceOpcode.jne, position, branchTarget, temp);
                        continue;
                    case 7:
                        branchTarget = sfa.ReadUInt24();
                        parse0x20(sfa, temp);
                        parse0x20(sfa, temp);
                        fileInstructions[position] = new sceInstruction(sceInstruction.sceOpcode.jbt, position, branchTarget, temp);
                        continue;
                    case 8:
                        branchTarget = sfa.ReadUInt24();
                        fileInstructions[position] = new sceInstruction(sceInstruction.sceOpcode.cll, position, branchTarget, temp);
                        continue;
                    case 9:
                        temp.Add(currOpcode);
                        parse0x40(sfa, temp);
                        fileInstructions[position] = new sceInstruction(sceInstruction.sceOpcode.not_implemented, sfa.GetPosition(), temp);
                        continue;
                    case 11:
                        temp.Add(currOpcode);
                        parse0x20(sfa, temp);
                        fileInstructions[position] = new sceInstruction(sceInstruction.sceOpcode.not_implemented, sfa.GetPosition(), temp);
                        continue;
                    case 0x47:
                        byte num = sfa.ReadByte();
                        uint strOff = 0;

                        switch (num >> 6)
                        {
                            case 0:
                                strOff = (uint)num & 0xF;
                                fileStrings.Add(new sceStrings((uint)sfa.PositionStream, offsetStrings)
                                {
                                    offset = strOff + offsetStrings,
                                    typeOffset = sceStrings.OffsetType.ShortOffset
                                });
                                break;
                            case 1:
                                strOff = (uint)(num & 0xF) << 8 | sfa.ReadByte();
                                fileStrings.Add(new sceStrings((uint)sfa.PositionStream, offsetStrings)
                                {
                                    offset = strOff + offsetStrings,
                                    typeOffset = sceStrings.OffsetType.MediumOffset
                                });
                                break;
                            case 2:
                                strOff = (uint)(num & 0xF) << 16 | sfa.ReadUInt16();
                                fileStrings.Add(new sceStrings((uint)sfa.PositionStream, offsetStrings)
                                {
                                    offset = strOff + offsetStrings,
                                    typeOffset = sceStrings.OffsetType.LargeOffset
                                });
                                break;
                            case 3:
                                strOff = (uint)(num & 0xF) << 24 | sfa.ReadUInt24();
                                break;
                        }

                        if ((num >> 4 & 3) != 1)
                        {
                            throw new InvalidOperationException();
                        }

                        if (sfa.ReadByte() != 0) {
                            throw new InvalidOperationException();
                        }

                        fileInstructions[position] = new sceInstruction
                        {
                            opcode = sceInstruction.sceOpcode.str,
                            offset = sfa.GetPosition(),
                            strTarget = strOff,
                            strExtra = 0,
                            size = 3 + (num >> 6)

                        };
                        fileStrings[fileStrings.Count - 1].instruction = fileInstructions[position];
                        continue;
                }

                if (currOpcode > 0x7F)
                {
                    sfa.SetPosition(sfa.GetPosition() - 1);
                    parse0x80(sfa, temp);
                    fileInstructions[position] = new sceInstruction(sceInstruction.sceOpcode.not_implemented, position, temp);
                    continue;
                }
                if (currOpcode > 0xF && currOpcode < 0x20)
                {
                    temp.Add(currOpcode);
                    int mask = (currOpcode >> 2) & 3;
                    if (mask == 1)
                    {
                        temp.Add(sfa.ReadByte());
                    }
                    if (mask == 2)
                    {
                        temp.Add(sfa.ReadByte());
                        temp.Add(sfa.ReadByte());
                    }
                    fileInstructions[position] = new sceInstruction(sceInstruction.sceOpcode.not_implemented, position, temp);
                    continue;
                }
                if (currOpcode > 0x1F && currOpcode < 0x30)
                {
                    sfa.SetPosition(sfa.GetPosition() - 1);
                    parse0x20(sfa, temp);
                    fileInstructions[position] = new sceInstruction(sceInstruction.sceOpcode.not_implemented, position, temp);
                    continue;
                }
                if (currOpcode > 0x2F && currOpcode < 0x40)
                {
                    temp.Add(currOpcode);
                    temp.Add(sfa.ReadByte()); temp.Add(sfa.ReadByte()); temp.Add(sfa.ReadByte()); temp.Add(sfa.ReadByte());
                    fileInstructions[position] = new sceInstruction(sceInstruction.sceOpcode.not_implemented, position, temp);
                    continue;
                }
                if (currOpcode > 0x3F && currOpcode < 0x80)
                {
                    sfa.SetPosition(sfa.GetPosition() - 1);
                    parse0x40(sfa, temp);
                    fileInstructions[position] = new sceInstruction(sceInstruction.sceOpcode.not_implemented, position, temp);
                    continue;
                }
            }
            return fileInstructions;
        }

        private void updateStringRef(sceInstruction inst, uint newTarget)
        {
            inst.strTarget = newTarget;
            if(newTarget <= 0x0F)
            {
                inst.size = 3 + 0; 
            }
            else if (newTarget <= 0xF_FF)
            {
                inst.size = 3 + 1;
            }
            else if (newTarget <= 0xF_FF_FF)
            {
                inst.size = 3 + 2;
            }
            else if (newTarget <= 0xF_FF_FF_FF)
            {
                inst.size = 3 + 3;
            }
        }

        public void WriteStrings(StreamFunctionAdd sfa, bool dedup = false)
        {
            sfa.PositionStream = offsetStrings;
            sfa.LengthStream = offsetStrings;
            bool matched = false;
            uint mockStringOffset = 0;
            uint off;
            for (int index = 0; index < fileStrings.Count; ++index)
            {
                if (dedup)
                {
                    for (int index1 = index - 1; index1 >= 0; --index1)
                    {
                        if (index == 0) break;
                        if (fileStrings[index].data.Count == fileStrings[index1].data.Count)
                        {
                            if (System.Linq.Enumerable.SequenceEqual(fileStrings[index].data, fileStrings[index1].data))
                            {
                                fileStrings[index].offset = fileStrings[index1].offset;
                                updateStringRef(fileStrings[index].instruction, fileStrings[index1].offset);
                                fileStrings[index].isDeduped = true;
                                matched = true;
                                break;
                            }
                        }
                    }
                    if (matched)
                        continue;
                }
                fileStrings[index].offset = mockStringOffset;
                updateStringRef(fileStrings[index].instruction, fileStrings[index].offset);
                mockStringOffset += (uint)fileStrings[index].data.Count;
            }

            int sizeStart = instructions[instructionOffsets[0]].size;
            for (int i = 1; i < instructionOffsets.Count; i++)
            {
                off = (uint)instructionOffsets[i];
                instructions[off].offset = sizeStart;
                sizeStart += instructions[off].size;
            }

            for (int i = 0; i < instructionOffsets.Count; i++)
            {
                off = (uint)instructionOffsets[i];
                if (instructions[off].opcode == sceInstruction.sceOpcode.not_implemented 
                    || instructions[off].opcode == sceInstruction.sceOpcode.str)
                {
                    continue;
                }
                uint tgt = instructions[off].branchTarget;
                instructions[off].branchTarget = (uint)instructions[tgt].offset;
            }

            sectionOffsets[0] = (uint)instructions[sectionOffsets[0]].offset;
            sectionOffsets[1] = (uint)instructions[sectionOffsets[1]].offset;
            for (int i = 2; i < sectionItemCount; i++)
            {
                sectionOffsets[i] = (uint)instructions[sectionOffsets[i]].offset;
            }

            sfa.SetPosition(offsetScript);
            
            for (int i = 0; i < instructionOffsets.Count; i++)
            {
                off = (uint)instructionOffsets[i];
                switch (instructions[off].opcode)
                {
                    case sceInstruction.sceOpcode.not_implemented:
                        sfa.WriteBytes(instructions[off].rawBytes);
                        continue;

                    case sceInstruction.sceOpcode.jmp:
                    case sceInstruction.sceOpcode.jzs:
                    case sceInstruction.sceOpcode.jnz:
                    case sceInstruction.sceOpcode.cll:
                    case sceInstruction.sceOpcode.jne:
                    case sceInstruction.sceOpcode.jbt:
                        sfa.WriteByte((byte)instructions[off].opcode);
                        sfa.WriteUInt24(instructions[off].branchTarget);
                        sfa.WriteBytes(instructions[off].operands);
                        break;

                    case sceInstruction.sceOpcode.str:
                        sfa.WriteByte((byte)instructions[off].opcode);
                        byte num = 0x10;
                        switch (instructions[off].size - 3)
                        {
                            case 0:
                                num |= 0 << 6;
                                num |= (byte)(instructions[off].strTarget & 0xF);
                                sfa.WriteByte(num);
                                break;
                            case 1:
                                num |= 1 << 6;
                                num |= (byte)((instructions[off].strTarget >> 8) & 0xF);
                                sfa.WriteByte(num);
                                sfa.WriteByte((byte)instructions[off].strTarget);
                                break;
                            case 2:
                                num |= 2 << 6;
                                num |= (byte)((instructions[off].strTarget >> 16) & 0xF);
                                sfa.WriteByte(num);
                                sfa.WriteUInt16((ushort)instructions[off].strTarget);
                                break;
                            case 3:
                                num |= 3 << 6;
                                num |= (byte)((instructions[off].strTarget >> 24) & 0xF);
                                sfa.WriteByte(num);
                                sfa.WriteUInt24(instructions[off].strTarget & 0xFF_FF_FF);
                                break;
                        }
                        sfa.WriteByte(0);
                        break;
                } 
            }
            sfa.WriteByte(0);
            offsetStrings = (uint)sfa.GetPosition();
            foreach (sceStrings str in fileStrings)
            {
                str.WriteData(sfa);
            }
            sfa.WriteByte(0);

            sfa.SetPosition(0xC);
            sfa.WriteUInt32(offsetStrings); sfa.ReadUInt32();
            sfa.WriteUInt32(sectionOffsets[0]);
            sfa.WriteUInt32(sectionOffsets[1]);
            sfa.SetPosition(0x34);
            for (int i = 2; i < sectionItemCount; i++)
            {
                sfa.ReadUInt32();
                sfa.WriteUInt32(sectionOffsets[i]);
            }
        }
    }
}
using StreamFAdd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sceWork
{
    internal class sceInstruction
    {
        public enum sceOpcode
        {
            not_implemented = -1,
            jmp = 3,
            jzs = 4,
            jnz = 5,
            jne = 6,
            jbt = 7,
            cll = 8,
            str = 0x47,
        }

        public sceOpcode opcode;
        public uint branchTarget;
        public uint strTarget;
        public byte strExtra;
        public long offset;
        public int size;
        public List<byte> operands;
        public List<byte> rawBytes;

        public sceInstruction(sceOpcode opcode, long originalOffset, List<byte> rawBytes)
        {
            this.opcode = opcode;
            this.offset = originalOffset;
            this.rawBytes = rawBytes;
            this.size = rawBytes.Count;
        }
        public sceInstruction(sceOpcode opcode, long originalOffset, uint branchTarget, List<byte> operands)
        {
            this.opcode = opcode;
            this.offset = originalOffset;
            this.branchTarget = branchTarget;
            this.operands = operands;
            this.size = operands.Count + 4;
        }

        public sceInstruction()
        {

        }
    }
}

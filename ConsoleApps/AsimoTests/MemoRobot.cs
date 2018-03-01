namespace AsimoTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Asimo;

    internal class MemoRobot : IRobot
    {
        private StringBuilder memo;

        public MemoRobot()
        {
            this.memo = new StringBuilder();
        }

        public void Beep()
        {
            Remember("b");
        }

        public void Move(double distance)
        {
            Remember($"m{distance}");
        }

        public void Turn(double angle)
        {
            Remember($"t{angle}");
        }

        public override string ToString() => this.memo.ToString();

        private void Remember(string instruction)
        {
            if (this.memo.Length > 0)
                this.memo.Append(";");
            this.memo.Append(instruction);
        }
    }
}
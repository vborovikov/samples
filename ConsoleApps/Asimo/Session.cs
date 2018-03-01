namespace Asimo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Session : IRobot
    {
        private readonly IRobot robot;
        private readonly List<Operation> instructions;

        public Session(IRobot robot)
        {
            this.robot = robot;
            this.instructions = new List<Operation>();
        }

        public static Session Open(IRobot robot)
        {
            return new Session(robot);
        }

        public void Beep()
        {
            this.robot.Beep();
            this.instructions.Add(new BeepOperation());
        }

        public void Move(double distance)
        {
            this.robot.Move(distance);
            this.instructions.Add(new MoveOperation(distance));
        }

        public void Turn(double angle)
        {
            this.robot.Turn(angle);
            this.instructions.Add(new TurnOperation(angle));
        }

        public Emulator Emulate()
        {
            return new Emulator(this.instructions.ToArray());
        }
    }

    public abstract class Operation
    {
        public abstract void Operate(IRobot robot);
    }

    public class Emulator : Operation
    {
        private readonly IReadOnlyList<Operation> instructions;

        internal Emulator(IReadOnlyList<Operation> instructions)
        {
            this.instructions = instructions;
        }

        public override void Operate(IRobot robot)
        {
            foreach (var operation in this.instructions)
            {
                operation.Operate(robot);
            }
        }

        public override string ToString()
        {
            return String.Join(";", this.instructions);
        }
    }

    internal class BeepOperation : Operation
    {
        public override void Operate(IRobot robot)
        {
            robot.Beep();
        }

        public override string ToString() => "b";
    }

    internal class MoveOperation : Operation
    {
        private readonly double distance;

        public MoveOperation(double distance)
        {
            this.distance = distance;
        }

        public override void Operate(IRobot robot)
        {
            robot.Move(this.distance);
        }

        public override string ToString() => $"m{this.distance}";
    }

    internal class TurnOperation : Operation
    {
        private readonly double angle;

        public TurnOperation(double angle)
        {
            this.angle = angle;
        }

        public override void Operate(IRobot robot)
        {
            robot.Turn(this.angle);
        }

        public override string ToString() => $"t{this.angle}";
    }
}
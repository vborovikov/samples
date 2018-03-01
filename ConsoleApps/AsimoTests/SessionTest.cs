namespace AsimoTests
{
    using System;
    using Asimo;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SessionTest
    {
        [TestMethod]
        public void EmulatorNotation()
        {
            var robot = new MemoRobot();
            var session = new Session(robot);

            // Operations
            session.Move(10);
            session.Turn(45);
            session.Move(15);
            session.Beep();
            session.Beep();

            var emulator = session.Emulate();

            // Robot and emulator should have the same instruction set
            Assert.AreEqual(robot.ToString(), emulator.ToString());
        }

        [TestMethod]
        public void EmulateRandomOperator()
        {
            var robotOriginal = new MemoRobot();
            var robotClone = new MemoRobot();

            var session = new Session(robotOriginal);
            RandomOperate(session);

            var emulator = session.Emulate();
            emulator.Operate(robotClone);

            // Both robots did the same thing
            Assert.AreEqual(robotOriginal.ToString(), robotClone.ToString());
        }

        [TestMethod]
        public void EmulatorInvariant()
        {
            var robotOriginal = new MemoRobot();
            var robot1 = new MemoRobot();
            var robot2 = new MemoRobot();

            var session = new Session(robotOriginal);
            RandomOperate(session);

            var emulator = session.Emulate();
            emulator.Operate(robot1);
            emulator.Operate(robot2);

            // Emulator repeated the instruction set twice
            Assert.AreEqual(robot1.ToString(), robot2.ToString());
            Assert.AreEqual(robotOriginal.ToString(), robot2.ToString());
        }

        private void RandomOperate(IRobot robot)
        {
            var random = new Random();

            for (var i = 0; i != 10; i++)
            {
                var op = random.Next(3);
                switch (op)
                {
                    case 0: robot.Beep(); break;
                    case 1: robot.Move(random.NextDouble() * 100d); break;
                    case 2: robot.Turn(random.NextDouble() * 360d); break;
                }
            }
        }
    }
}
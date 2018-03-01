namespace Asimo
{
    public interface IRobot
    {
        void Move(double distance);

        void Turn(double angle);

        void Beep();
    }
}
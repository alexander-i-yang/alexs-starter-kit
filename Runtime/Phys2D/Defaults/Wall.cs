namespace ASK.Runtime.Phys2D
{
    public class Wall : Solid
    {
        public override bool Collidable(PhysObj collideWith)
        {
            return true;
        }
    }
}
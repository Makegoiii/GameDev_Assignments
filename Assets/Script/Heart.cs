public class Heart : Hazard
{
    protected override void OnHitPlayer(Health player)
    {
        player.Heal(Items.heartHeal);
    }
}

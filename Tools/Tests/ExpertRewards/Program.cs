using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Items;
using XianXia.Common.Players;
int assertions=0;
void Check(bool ok,string message){assertions++;if(!ok)throw new Exception(message);}
bool Close(float a,float b)=>Math.Abs(a-b)<0.0001f;
var gate=new ExpertRewardPlayer();
for(int i=0;i<12;i++){Check(gate.TryActivate(i),"First copy should activate");Check(!gate.TryActivate(i),"Duplicate copies must not stack");}
Check(!gate.TryActivate(-1)&&!gate.TryActivate(12),"Unknown rewards must not activate");
gate.ResetEffects();for(int i=0;i<12;i++)Check(gate.TryActivate(i),"Effects must refresh next tick");
for(int i=0;i<12;i++){
 var p=new Player();var c=new XianXiaPlayer();ExpertRewardEffects.Apply(p,c,i);
 bool ok=i switch {
 0=>c.spiritualEnergyRegenBonus==1&&p.statDefense==2,
 1=>p.lifeRegen==2&&p.buffImmune[BuffID.Poisoned],
 2=>p.statDefense==6&&p.buffImmune[BuffID.OnFire],
 3=>Close(c.spiritualEnergyCostMultiplier,.92f)&&Close(p.endurance,.03f),
 4=>Close(p.moveSpeed,.12f)&&Close(p.jumpSpeedBoost,1.5f),
 5=>c.spiritualEnergyRegenBonus==2&&Close(p.GetDamage(DamageClass.Magic),.06f)&&p.GetDamage(DamageClass.Generic)==0,
 6=>Close(p.GetDamage(DamageClass.Melee),.08f)&&p.GetArmorPenetration(DamageClass.Melee)==8,
 7=>p.lifeRegen==4&&c.spiritualEnergyRegenBonus==1,
 8=>Close(p.endurance,.06f)&&p.noKnockback,
 9=>Close(p.GetDamage(DamageClass.Ranged),.08f)&&p.GetArmorPenetration(DamageClass.Ranged)==8,
 10=>p.maxMinions==1&&Close(p.GetDamage(DamageClass.Summon),.08f),
 11=>Close(p.GetDamage(DamageClass.Generic),.10f)&&p.statDefense==6&&c.spiritualEnergyRegenBonus==2,
 _=>false};Check(ok,$"Reward {i} actual effects");
}
var unchanged=new Player();var ordinary=new XianXiaPlayer();ExpertRewardEffects.Apply(unchanged,ordinary,99);Check(unchanged.statDefense==0&&ordinary.spiritualEnergyRegenBonus==0&&ordinary.spiritualEnergyCostMultiplier==1,"Unknown effects are inert");
var first=new ExpertRewardPlayer();var second=new ExpertRewardPlayer();Check(first.TryActivate(0)&&second.TryActivate(0),"Players must have independent effects");
Console.WriteLine($"Expert reward source regression passed: {assertions} assertions; mocked engine boundary.");

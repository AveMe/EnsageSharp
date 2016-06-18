﻿namespace DotAllCombo.Heroes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Extensions;
    using Ensage.Common.Menu;
    using Extensions;

    internal class LinaController : HeroController
    {
        private static Ability Q, W, R;
        private static Hero me, target;
        private static Menu menu;
        private static readonly Menu skills = new Menu("Skills", "Skills");
        private static readonly Menu items = new Menu("Items", "Items");
        private static readonly Menu link = new Menu("Link", "Link");
        private static readonly Menu ult = new Menu("AutoUlt", "AutoUlt");
        private static bool Active;

        private static Item orchid,
            sheep,
            vail,
            soulring,
            arcane,
            blink,
            shiva,
            dagon,
            atos,
            ethereal,
            cheese,
            ghost,
            force,
            cyclone;

        private static int[] rDmg;

        public override void OnInject()
        {
            AssemblyExtensions.InitAssembly("NeverMore", "0.1");

            DebugExtensions.Chat.PrintSuccess("To destroy the darkness in itself!");

            menu = MenuExtensions.GetMenu();
            me = Variables.me;
            menu.AddItem(new MenuItem("enabled", "Enabled").SetValue(true));
            menu.AddItem(new MenuItem("keyBind", "Combo key").SetValue(new KeyBind('D', KeyBindType.Press)));

            var Skills = new Dictionary<string, bool>
            {
                {"lina_dragon_slave", true},
                {"lina_light_strike_array", true},
                {"lina_laguna_blade", true}
            };
            var AutoUlt = new Dictionary<string, bool>
            {
                {"lina_laguna_blade", true}
            };
            var Link = new Dictionary<string, bool>
            {
                {"item_force_staff", true},
                {"item_cyclone", true},
                {"item_orchid", true},
                {"item_rod_of_atos", true},
            };
            var Items = new Dictionary<string, bool>
            {
                {"item_cyclone", true},
                {"item_orchid", true},
                {"item_ethereal_blade", true},
                {"item_veil_of_discord", true},
                {"item_rod_of_atos", true},
                {"item_sheepstick", true},
                {"item_arcane_boots", true},
                {"item_blink", true},
				{"item_shivas_guard", true},
				{"item_soul_ring", true},
                {"item_ghost", true},
                {"item_cheese", true}
            };
			

			skills.AddItem(new MenuItem("Skills", "Skills").SetValue(new AbilityToggler(Skills)));
            items.AddItem(new MenuItem("Items", "Items:").SetValue(new AbilityToggler(Items)));
            ult.AddItem(new MenuItem("AutoUlt: ", "AutoUlt").SetValue(new AbilityToggler(AutoUlt)));
            items.AddItem(new MenuItem("Link: ", "Auto triggre Linken").SetValue(new AbilityToggler(Link)));
            menu.AddSubMenu(skills);
            menu.AddSubMenu(items);
            menu.AddSubMenu(ult);
        }

        public override void OnUpdateEvent(EventArgs args)
        {
            target = me.ClosestToMouseTarget(2000);
            if (target == null)
            {
                return;
            }
            if (!menu.Item("enabled").IsActive())
                return;
            //spell
            Q = me.Spellbook.SpellQ;

            W = me.Spellbook.SpellW;

            R = me.Spellbook.SpellR;

            // Item
            ethereal = me.FindItem("item_ethereal_blade");

            sheep = target.ClassID == ClassID.CDOTA_Unit_Hero_Tidehunter ? null : me.FindItem("item_sheepstick");

            vail = me.FindItem("item_veil_of_discord");

            cheese = me.FindItem("item_cheese");

            ghost = me.FindItem("item_ghost");

            orchid = me.FindItem("item_orchid");

            atos = me.FindItem("item_rod_of_atos");

            soulring = me.FindItem("item_soul_ring");

            arcane = me.FindItem("item_arcane_boots");

            blink = me.FindItem("item_blink");

            shiva = me.FindItem("item_shivas_guard");

			dagon = me.Inventory.Items.FirstOrDefault(item => item.Name.Contains("item_dagon"));

            force = me.FindItem("item_force_staff");

            cyclone = me.FindItem("item_cyclone");

            orchid = me.FindItem("item_orchid");

            atos = me.FindItem("item_rod_of_atos");
            //me.Inventory.Items.FirstOrDefault(item => item.Name.Contains("item_dagon"));


            Active = Game.IsKeyDown(menu.Item("keyBind").GetValue<KeyBind>().Key) && !Game.IsChatOpen;

            var ModifRod = target.Modifiers.Any(y => y.Name == "modifier_rod_of_atos_debuff");
            var ModifEther = target.Modifiers.Any(y => y.Name == "modifier_item_ethereal_blade_slow");
            var ModifVail = target.Modifiers.Any(y => y.Name == "modifier_item_veil_of_discord_debuff");
            var stoneModif = target.Modifiers.Any(y => y.Name == "modifier_medusa_stone_gaze_stone");
            var EulModifier = target.Modifiers.FirstOrDefault(x => x.Name == "modifier_eul_cyclone");
			var EulModif = target.Modifiers.Any(x => x.Name == "modifier_eul_cyclone");
			var noBlade = target.Modifiers.Any(y => y.Name == "modifier_item_blade_mail_reflect");
			if (me.IsAlive)
            {
                if (Active && target.IsVisible && me.Distance2D(target) <= 2300 && !noBlade)
				{
					if (
						blink != null
						&& me.CanCast()
						&& blink.CanBeCasted()
						&& me.Distance2D(target) > 1000
						&& menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(blink.Name)
						&& Utils.SleepCheck("blink")
						)
					{
						blink.UseAbility(target.Position);
						Utils.Sleep(250, "blink");
					}
					if (
						cyclone != null 
						&& cyclone.CanBeCasted() 
						&& me.Distance2D(target) <= 900 
						&& menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(cyclone.Name) 
						&& W.CanBeCasted()
						&& Utils.SleepCheck("cyclone"))
                    {
                        cyclone.UseAbility(target);
                        Utils.Sleep(300, "cyclone");
                    }
                    if (cyclone == null || !cyclone.CanBeCasted() ||!W.CanBeCasted() ||
                        !menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(cyclone.Name))
                    {
                        if (
                            W != null
                            &&
                            (EulModifier.RemainingTime < 0.80 || cyclone == null ||
                             !menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(cyclone.Name))
                            && W.CanBeCasted()
                            && me.CanCast()
                            && me.Distance2D(target) < 525
                            && menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(W.Name)
                            && Utils.SleepCheck("W"))
                        {
                            W.UseAbility(target.Predict(450));
                            Utils.Sleep(300, "W");
                        }
                        if ( // atos Blade
                            atos != null
                            && atos.CanBeCasted()
                            && me.CanCast()
                            && !target.IsLinkensProtected()
                            && !target.IsMagicImmune()
                            && menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(atos.Name)
                            && me.Distance2D(target) <= 2000
                            && Utils.SleepCheck("atos")
                            )
                        {
                            atos.UseAbility(target);

                            Utils.Sleep(250, "atos");
                        } // atos Item end

                        if ( // orchid
                            orchid != null
                            && orchid.CanBeCasted()
                            && me.CanCast()
                            && !target.IsLinkensProtected()
                            && !target.IsMagicImmune()
                            && me.Distance2D(target) <= 1400
                            && menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(orchid.Name)
                            && !stoneModif
                            && Utils.SleepCheck("orchid")
                            )
                        {
                            orchid.UseAbility(target);
                            Utils.Sleep(250, "orchid");
                        } // orchid Item end
                        if (!orchid.CanBeCasted() || orchid == null ||
                            !menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(orchid.Name))
                        {
                            if ( // vail
                                vail != null
                                && vail.CanBeCasted()
                                && me.CanCast()
                                && !target.IsMagicImmune()
                                && menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(vail.Name)
                                && me.Distance2D(target) <= 1500
                                && Utils.SleepCheck("vail")
                                )
                            {
                                vail.UseAbility(target.Position);
                                Utils.Sleep(250, "vail");
                            } // orchid Item end
                            if (!vail.CanBeCasted() || vail == null ||
                                !menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(vail.Name))
                            {
                                if ( // ethereal
                                    ethereal != null
                                    && ethereal.CanBeCasted()
                                    && me.CanCast()
                                    && !target.IsLinkensProtected()
                                    && !target.IsMagicImmune()
                                    && !stoneModif
                                    && menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(ethereal.Name)
                                    && Utils.SleepCheck("ethereal")
                                    )
                                {
                                    ethereal.UseAbility(target);
                                    Utils.Sleep(200, "ethereal");
                                } // ethereal Item end
                                if (!ethereal.CanBeCasted() || ethereal == null ||
                                    !menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(ethereal.Name))
                                {
                                    if (
                                        Q != null
                                        && Q.CanBeCasted()
                                        && me.CanCast()
                                        && me.Distance2D(target) < 1400
                                        && !stoneModif
										&& !target.IsMagicImmune()
										&& menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(Q.Name)
                                        && Utils.SleepCheck("Q")
                                        )
                                    {
                                        Q.UseAbility(target);
                                        Utils.Sleep(200, "Q");
                                    }

                                    if (
                                        R != null
                                        && R.CanBeCasted()
                                        && !Q.CanBeCasted()
                                        && !W.CanBeCasted()
                                        && me.CanCast()
                                        && me.Position.Distance2D(target) < 1200
                                        && !stoneModif
                                        && menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(R.Name)
                                        && Utils.SleepCheck("R"))
                                    {
                                        R.UseAbility(target);
                                        Utils.Sleep(330, "R");
                                    }

                                    if ( // SoulRing Item 
                                        soulring != null
                                        && soulring.CanBeCasted()
                                        && me.CanCast()
                                        && me.Health >= (me.MaximumHealth*0.4)
                                        && me.Mana <= R.ManaCost
                                        && menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(soulring.Name)
                                        )
                                    {
                                        soulring.UseAbility();
                                    } // SoulRing Item end

                                    if ( // Arcane Boots Item
                                        arcane != null
                                        && arcane.CanBeCasted()
                                        && me.CanCast()
                                        && me.Mana <= R.ManaCost
                                        && menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(arcane.Name)
                                        )
                                    {
                                        arcane.UseAbility();
                                    } // Arcane Boots Item end

                                    if ( //Ghost
                                        ghost != null
                                        && ghost.CanBeCasted()
                                        && me.CanCast()
                                        && ((me.Position.Distance2D(target) < 300
                                             && me.Health <= (me.MaximumHealth*0.7))
                                            || me.Health <= (me.MaximumHealth*0.3))
                                        && menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(ghost.Name)
                                        && Utils.SleepCheck("Ghost"))
                                    {
                                        ghost.UseAbility();
                                        Utils.Sleep(250, "Ghost");
                                    }


                                    if ( // Shiva Item
                                        shiva != null
                                        && shiva.CanBeCasted()
                                        && me.CanCast()
                                        && !target.IsMagicImmune()
                                        && Utils.SleepCheck("shiva")
                                        && menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(shiva.Name)
                                        && me.Distance2D(target) <= 600
                                        )

                                    {
                                        shiva.UseAbility();
                                        Utils.Sleep(250, "shiva");
                                    } // Shiva Item end


                                    if ( // sheep
                                        sheep != null
                                        && sheep.CanBeCasted()
                                        && me.CanCast()
                                        && !target.IsLinkensProtected()
                                        && !target.IsMagicImmune()
                                        && me.Distance2D(target) <= 1400
                                        && !stoneModif
                                        && menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(sheep.Name)
                                        && Utils.SleepCheck("sheep")
                                        )
                                    {
                                        sheep.UseAbility(target);
                                        Utils.Sleep(250, "sheep");
                                    } // sheep Item end

                                    if ( // Dagon
                                        me.CanCast()
                                        && dagon != null
                                        && (ethereal == null
                                            || (ModifEther
                                                || ethereal.Cooldown < 17))
                                        && !target.IsLinkensProtected()
                                        && dagon.CanBeCasted()
                                        && !target.IsMagicImmune()
                                        && !stoneModif
                                        && Utils.SleepCheck("dagon")
                                        )
                                    {
                                        dagon.UseAbility(target);
                                        Utils.Sleep(200, "dagon");
                                    } // Dagon Item end

                                    if (
                                        // cheese
                                        cheese != null
                                        && cheese.CanBeCasted()
                                        && me.Health <= (me.MaximumHealth*0.3)
                                        && me.Distance2D(target) <= 700
                                        && menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(cheese.Name)
                                        && Utils.SleepCheck("cheese")
                                        )
                                    {
                                        cheese.UseAbility();
                                        Utils.Sleep(200, "cheese");
                                    } // cheese Item end
									if (
									(!me.CanAttack()
									|| me.Distance2D(target) >= 350)
									&& me.NetworkActivity != NetworkActivity.Attack
									&& me.Distance2D(target) <= 900
									&& Utils.SleepCheck("Move"))
									{
										me.Move(target.Predict(500));
										Utils.Sleep(350, "Move");
									}
									if (
										me.Distance2D(target) <= 100
										&& me.CanAttack()
										&& me.NetworkActivity != NetworkActivity.Attack
										&& Utils.SleepCheck("R")
										)
									{
										me.Attack(target);
										Utils.Sleep(200, "R");
									}
								}
                            }
                        }
                    }
				}

				A();
			}
		}

        private static void A()
        {
          
            if (menu.Item("AutoUlt: ").GetValue<AbilityToggler>().IsEnabled(R.Name) && R.CanBeCasted())
			{
				var enemies =
				 ObjectManager.GetEntities<Hero>()
					 .Where(x => x.IsVisible && x.IsAlive && x.Team == me.GetEnemyTeam() && !x.IsIllusion);
				foreach (var e in enemies)
                {
                    if (e == null)
                        return;

                    if (me.AghanimState())
                        rDmg = new int[3] {450, 650, 850};


                    var leans = me.FindItem("item_aether_lens");
					var agh = (rDmg[R.Level - 1]);
                    var damage = Math.Floor(rDmg[R.Level - 1]*(1 - e.MagicDamageResist));
                    if (e.NetworkName == "CDOTA_Unit_Hero_Spectre" && e.Spellbook.Spell3.Level > 0)
                    {
                        damage =
                            Math.Floor(rDmg[R.Level - 1]*
                                       (1 - (0.10 + e.Spellbook.Spell3.Level*0.04))*(1 - e.MagicDamageResist));
                    }
                    if (e.NetworkName == "CDOTA_Unit_Hero_SkeletonKing" &&
                        e.Spellbook.SpellR.CanBeCasted())
                        damage = 0;
                    if (e.NetworkName == "CDOTA_Unit_Hero_Tusk" &&
                        e.Spellbook.SpellW.CooldownLength - 3 > e.Spellbook.SpellQ.Cooldown)
                        damage = 0;
                    if (leans != null) damage = damage*1.08;
                    var rum = e.Modifiers.Any(x => x.Name == "modifier_kunkka_ghost_ship_damage_absorb");
                    if (rum) damage = damage*0.5;
                    var mom = e.Modifiers.Any(x => x.Name == "modifier_item_mask_of_madness_berserk");
                    if (mom) damage = damage*1.3;

                    if (!me.AghanimState() && !e.IsLinkensProtected())
                    {
                        if (vail != null && e.Health <= (damage*1.25)
                            && vail.CanBeCasted()
                            && me.Distance2D(e) <= R.CastRange + 30
                            && R.CanBeCasted()
                            && me.Distance2D(e) <= 1200
                            && Utils.SleepCheck(e.Handle.ToString())
                            )
                        {
                            vail.UseAbility(e.Position);
                            Utils.Sleep(500, e.Handle.ToString());
                            goto leave;
                        }

                        if (ethereal != null && e.Health <= (damage*1.5)
                            && me.Distance2D(e) <= ethereal.CastRange + 30
                            && R.CanBeCasted()
                            && !e.IsMagicImmune()
                            && ethereal.CanBeCasted()
                            && !e.IsLinkensProtected()
                            && me.Distance2D(e) <= 1000
                            && Utils.SleepCheck(e.Handle.ToString())
                            )
                        {
                            ethereal.UseAbility(e);
                            Utils.Sleep(500, e.Handle.ToString());
                            goto leave;
                        }
                        if (R != null && e != null && R.CanBeCasted()
                            && !e.Modifiers.Any(y => y.Name == "modifier_tusk_snowball_movement")
                            && !e.Modifiers.Any(y => y.Name == "modifier_snowball_movement_friendly")
                            && !e.Modifiers.Any(y => y.Name == "modifier_templar_assassin_refraction_absorb")
                            && !e.Modifiers.Any(y => y.Name == "modifier_ember_spirit_flame_guard")
                            &&
                            !e.Modifiers.Any(
                                y => y.Name == "modifier_ember_spirit_sleight_of_fist_caster_invulnerability")
                            &&
                            !e.Modifiers.Any(
                                y => y.Name == "modifier_ember_spirit_sleight_of_fist_caster_invulnerability")
                            && !e.Modifiers.Any(y => y.Name == "modifier_obsidian_destroyer_astral_imprisonment_prison")
                            && !e.Modifiers.Any(y => y.Name == "modifier_puck_phase_shift")
                            && !e.Modifiers.Any(y => y.Name == "modifier_eul_cyclone")
                            && !e.Modifiers.Any(y => y.Name == "modifier_dazzle_shallow_grave")
                            && !e.Modifiers.Any(y => y.Name == "modifier_shadow_demon_disruption")
                            && !e.Modifiers.Any(y => y.Name == "modifier_necrolyte_reapers_scythe")
                            && !e.Modifiers.Any(y => y.Name == "modifier_storm_spirit_ball_lightning")
                            && !e.Modifiers.Any(y => y.Name == "modifier_ember_spirit_fire_remnant")
                            && !e.Modifiers.Any(y => y.Name == "modifier_nyx_assassin_spiked_carapace")
                            && !e.Modifiers.Any(y => y.Name == "modifier_phantom_lancer_doppelwalk_phase")
                            && !e.FindSpell("abaddon_borrowed_time").CanBeCasted() &&
                            !e.Modifiers.Any(y => y.Name == "modifier_abaddon_borrowed_time_damage_redirect")
                            && !e.IsMagicImmune()
                            && e.Health <= (damage - 40 - e.HealthRegeneration)
                            && Utils.SleepCheck(e.Handle.ToString())
                            )
                        {
                            R.UseAbility(e);
                            Utils.Sleep(150, e.Handle.ToString());
                            return;
                        }
                        if (me.AghanimState() && !e.IsLinkensProtected())
                        {
                            if (R != null && e != null && R.CanBeCasted()
                                && !e.Modifiers.Any(y => y.Name == "modifier_tusk_snowball_movement")
                                && !e.Modifiers.Any(y => y.Name == "modifier_snowball_movement_friendly")
                                && !e.Modifiers.Any(y => y.Name == "modifier_templar_assassin_refraction_absorb")
                                && !e.Modifiers.Any(y => y.Name == "modifier_ember_spirit_flame_guard")
                                &&
                                !e.Modifiers.Any(
                                    y => y.Name == "modifier_ember_spirit_sleight_of_fist_caster_invulnerability")
                                &&
                                !e.Modifiers.Any(
                                    y => y.Name == "modifier_ember_spirit_sleight_of_fist_caster_invulnerability")
                                &&
                                !e.Modifiers.Any(y => y.Name == "modifier_obsidian_destroyer_astral_imprisonment_prison")
                                && !e.Modifiers.Any(y => y.Name == "modifier_puck_phase_shift")
                                && !e.Modifiers.Any(y => y.Name == "modifier_eul_cyclone")
                                && !e.Modifiers.Any(y => y.Name == "modifier_dazzle_shallow_grave")
                                && !e.Modifiers.Any(y => y.Name == "modifier_shadow_demon_disruption")
                                && !e.Modifiers.Any(y => y.Name == "modifier_necrolyte_reapers_scythe")
                                && !e.Modifiers.Any(y => y.Name == "modifier_storm_spirit_ball_lightning")
                                && !e.Modifiers.Any(y => y.Name == "modifier_ember_spirit_fire_remnant")
                                && !e.Modifiers.Any(y => y.Name == "modifier_nyx_assassin_spiked_carapace")
                                && !e.Modifiers.Any(y => y.Name == "modifier_phantom_lancer_doppelwalk_phase")
                                && !e.FindSpell("abaddon_borrowed_time").CanBeCasted() &&
                                !e.Modifiers.Any(y => y.Name == "modifier_abaddon_borrowed_time_damage_redirect")
                                && !e.IsMagicImmune()
                                && menu.Item("AutoUlt: ").GetValue<AbilityToggler>().IsEnabled(R.Name)
                                && e.Health <= (agh - e.HealthRegeneration)
                                && Utils.SleepCheck(e.Handle.ToString())
                                )
                            {
                                R.UseAbility(e);
                                Utils.Sleep(150, e.Handle.ToString());
                                return;
                            }
                        }
                    }
                    if (e.IsLinkensProtected() &&
                        (me.IsVisibleToEnemies || Game.IsKeyDown(menu.Item("Combo Key").GetValue<KeyBind>().Key)))
                    {
                        if (force != null && force.CanBeCasted() && me.Distance2D(e) < force.CastRange &&
                            menu.Item("Link: ").GetValue<AbilityToggler>().IsEnabled(force.Name) &&
                            Utils.SleepCheck(e.Handle.ToString()))
                        {
                            force.UseAbility(e);
                            Utils.Sleep(500, e.Handle.ToString());
                        }
                        else if (orchid != null && orchid.CanBeCasted() && me.Distance2D(e) < orchid.CastRange &&
                                 menu.Item("Link: ").GetValue<AbilityToggler>().IsEnabled(orchid.Name) &&
                                 Utils.SleepCheck(e.Handle.ToString()))
                        {
                            orchid.UseAbility(e);
                            Utils.Sleep(500, e.Handle.ToString());
                        }
                        else if (atos != null && atos.CanBeCasted() && me.Distance2D(e) < atos.CastRange - 400 &&
                                 menu.Item("Link: ").GetValue<AbilityToggler>().IsEnabled(atos.Name) &&
                                 Utils.SleepCheck(e.Handle.ToString()))
                        {
                            atos.UseAbility(e);
                            Utils.Sleep(500, e.Handle.ToString());
                        }
                        else if (dagon != null && dagon.CanBeCasted() && me.Distance2D(e) < dagon.CastRange &&
                                 menu.Item("Link: ").GetValue<AbilityToggler>().IsEnabled(dagon.Name) &&
                                 Utils.SleepCheck(e.Handle.ToString()))
                        {
                            dagon.UseAbility(e);
                            Utils.Sleep(500, e.Handle.ToString());
                        }
                        else if (cyclone != null && cyclone.CanBeCasted() &&
                                 me.Distance2D(e) < cyclone.CastRange &&
                                 menu.Item("Link: ").GetValue<AbilityToggler>().IsEnabled(cyclone.Name) &&
                                 Utils.SleepCheck(e.Handle.ToString()))
                        {
                            cyclone.UseAbility(e);
                            Utils.Sleep(500, e.Handle.ToString());
                        }
                    }
                }
                leave:
                ;
            }
        }
    }
}
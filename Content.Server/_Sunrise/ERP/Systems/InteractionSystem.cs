// © SUNRISE, An EULA/CLA with a hosting restriction, full text: https://github.com/space-sunrise/lust-station/blob/master/CLA.txt
using Content.Shared._Sunrise.ERP.Components;
using Content.Shared.Database;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using Robust.Server.GameObjects;
using Robust.Shared.Random;
using Content.Server.EUI;
using Content.Shared.Humanoid;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using Content.Server.Chat.Systems;
namespace Content.Server._Sunrise.ERP.Systems
{
    public sealed class InteractionSystem : EntitySystem
    {
        [Dependency] private readonly EuiManager _eui = default!;
        [Dependency] protected readonly ItemSlotsSystem ItemSlotsSystem = default!;
        [Dependency] protected readonly IGameTiming _gameTiming = default!;
        [Dependency] protected readonly ChatSystem _chat = default!;
        [Dependency] protected readonly IRobustRandom _random = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<InteractionComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<InteractionComponent, GetVerbsEvent<Verb>>(AddVerbs);
        }

        public (Sex, bool, Sex, bool, bool)? RequestMenu(EntityUid User, EntityUid Target)
        {
            if (TryComp<InteractionComponent>(Target, out var targetInteraction) && TryComp<InteractionComponent>(User, out var userInteraction))
            {
                if (TryComp<HumanoidAppearanceComponent>(Target, out var targetHumanoid) && TryComp<HumanoidAppearanceComponent>(User, out var userHumanoid))
                {
                    bool erp = true;
                    bool userClothing = false;
                    bool targetClothing = false;
                    if (!targetInteraction.Erp || !userInteraction.Erp) erp = false;
                    if (TryComp<ContainerManagerComponent>(User, out var container))
                    {
                        if (container.Containers["jumpsuit"].ContainedEntities.Count != 0) userClothing = true;
                        if (container.Containers["outerClothing"].ContainedEntities.Count != 0) userClothing = true;
                    }

                    if (TryComp<ContainerManagerComponent>(Target, out var targetContainer))
                    {
                        if (targetContainer.Containers["jumpsuit"].ContainedEntities.Count != 0) targetClothing = true;
                        if (targetContainer.Containers["outerClothing"].ContainedEntities.Count != 0) targetClothing = true;
                    }
                    return (userHumanoid.Sex, userClothing, targetHumanoid.Sex, targetClothing, erp);
                }
            }
            return null;
        }

        public void AddLove(NetEntity entity, NetEntity target, int percentUser, int percentTarget)
        {
            var User = GetEntity(entity);
            var Target = GetEntity(target);
            if (!TryComp<InteractionComponent>(User, out var compUser)) return;
            if (!TryComp<InteractionComponent>(Target, out var compTarget)) return;

            if (percentUser != 0)
            {
                if (_gameTiming.CurTime > compUser.LoveDelay)
                {
                    compUser.ActualLove += (percentUser + _random.Next(-percentUser / 2, percentUser / 2)) / 100f;
                    compUser.TimeFromLastErp = _gameTiming.CurTime;
                }
                Spawn("EffectHearts", Transform(User).Coordinates);
                if(_random.Prob(0.1f))
                {
                    _chat.TryEmoteWithChat(User, "Moan", ChatTransmitRange.Normal);
                }
            }
            if (compUser.Love >= 1)
            {
                compUser.ActualLove = 0;
                compUser.Love = 0.95f;
                compUser.LoveDelay = _gameTiming.CurTime + TimeSpan.FromMinutes(1);
                _chat.TrySendInGameICMessage(User, "кончает!", InGameICChatType.Emote, false);
                if(TryComp<HumanoidAppearanceComponent>(User, out var humuser))
                {
                    if(humuser.Sex == Sex.Male)
                    {
                        Spawn("PuddleSemen", Transform(User).Coordinates);
                    }
                }
            }

            if (percentTarget != 0)
            {
                if (_gameTiming.CurTime > compTarget.LoveDelay)
                {
                    compTarget.ActualLove += (percentTarget + _random.Next(-percentTarget / 2, percentTarget / 2)) / 100f;
                    compTarget.TimeFromLastErp = _gameTiming.CurTime;
                }
                Spawn("EffectHearts", Transform(Target).Coordinates);
                if (_random.Prob(0.1f))
                {
                    _chat.TryEmoteWithChat(User, "Moan", ChatTransmitRange.Normal);
                }
            }
            if (compTarget.Love >= 1)
            {
                compTarget.ActualLove = 0;
                compTarget.Love = 0.95f;
                compTarget.LoveDelay = _gameTiming.CurTime + TimeSpan.FromMinutes(1);
                _chat.TrySendInGameICMessage(Target, "кончает!", InGameICChatType.Emote, false);
                if (TryComp<HumanoidAppearanceComponent>(Target, out var taruser))
                {
                    if (taruser.Sex == Sex.Male)
                    {
                        Spawn("PuddleSemen", Transform(Target).Coordinates);
                    }
                }
            }
        }
        private void AddVerbs(EntityUid uid, InteractionComponent comp, GetVerbsEvent<Verb> args)
        {
            if (!EntityManager.TryGetComponent(args.User, out ActorComponent? actor))
                return;


            var player = actor.PlayerSession;
            if (!args.CanInteract || !args.CanAccess) return;
            args.Verbs.Add(new Verb
            {
                Priority = -1,
                Text = "Взаимодействовать с...",
                Icon = new SpriteSpecifier.Texture(new("/Textures/_Sunrise/Interface/ERP/heart.png")),
                Act = () =>
                {
                    if (!args.CanInteract || !args.CanAccess) return;
                    if (TryComp<InteractionComponent>(args.Target, out var targetInteraction) && TryComp<InteractionComponent>(args.User, out var userInteraction))
                    {
                        if (TryComp<HumanoidAppearanceComponent>(args.Target, out var targetHumanoid) && TryComp<HumanoidAppearanceComponent>(args.User, out var userHumanoid))
                        {
                            bool erp = true;
                            bool userClothing = false;
                            bool targetClothing = false;
                            if (!targetInteraction.Erp || !userInteraction.Erp) erp = false;
                            if (TryComp<ContainerManagerComponent>(args.User, out var container))
                            {
                                if (container.Containers["jumpsuit"].ContainedEntities.Count != 0) userClothing = true;
                                if (container.Containers["outerClothing"].ContainedEntities.Count != 0) userClothing = true;
                            }

                            if (TryComp<ContainerManagerComponent>(args.Target, out var targetContainer))
                            {
                                if (targetContainer.Containers["jumpsuit"].ContainedEntities.Count != 0) targetClothing = true;
                                if (targetContainer.Containers["outerClothing"].ContainedEntities.Count != 0) targetClothing = true;
                            }

                            _eui.OpenEui(new InteractionEui(GetNetEntity(args.Target), userHumanoid.Sex, userClothing, targetHumanoid.Sex, targetClothing, erp), player);
                        }
                    }
                },
                Impact = LogImpact.Low,
            });
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = EntityQueryEnumerator<InteractionComponent>();
            while (query.MoveNext(out var uid, out var comp))
            {
                comp.Love -= ((comp.Love - comp.ActualLove) / 1) * frameTime;
                if (_gameTiming.CurTime - comp.TimeFromLastErp > TimeSpan.FromSeconds(40) && comp.Love > 0)
                {
                    comp.ActualLove -= 0.001f;
                }
                if (comp.Love < 0) comp.Love = 0;
                if (comp.ActualLove < 0) comp.ActualLove = 0;
            }
        }

        private void OnComponentInit(EntityUid uid, InteractionComponent component, ComponentInit args)
        {
        }
    }
}

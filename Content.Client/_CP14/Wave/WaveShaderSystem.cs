using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Client._CP14.Wave;

public sealed class WaveShaderSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private ShaderInstance _shader = default!;

    public override void Initialize()
    {
        base.Initialize();

        _shader = _protoMan.Index<ShaderPrototype>("Wave").InstanceUnique();

        SubscribeLocalEvent<WaveShaderComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<WaveShaderComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<WaveShaderComponent, BeforePostShaderRenderEvent>(OnBeforeShaderPost);
    }

    private void OnStartup(Entity<WaveShaderComponent> entity, ref ComponentStartup args)
    {
        entity.Comp.Offset = _random.NextFloat(0, 1000);
        SetShader(entity.Owner, _shader);
    }

    private void OnShutdown(Entity<WaveShaderComponent> entity, ref ComponentShutdown args)
    {
        SetShader(entity.Owner, null);
    }

    private void SetShader(Entity<SpriteComponent?> entity, ShaderInstance? instance)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        entity.Comp.PostShader = instance;
        entity.Comp.GetScreenTexture = instance is not null;
        entity.Comp.RaiseShaderEvent = instance is not null;
    }

    private void OnBeforeShaderPost(Entity<WaveShaderComponent> entity, ref BeforePostShaderRenderEvent args)
    {
        _shader.SetParameter("Speed", entity.Comp.Speed);
        _shader.SetParameter("Dis", entity.Comp.Dis);
        _shader.SetParameter("Offset", entity.Comp.Offset);
    }
}
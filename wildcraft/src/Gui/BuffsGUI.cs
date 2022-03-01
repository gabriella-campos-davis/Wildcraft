using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using System;
using System.Collections;
using BuffStuff;

namespace wildcraft.Gui{
public class HudElementBuffs : HudElement {

    public override double InputOrder => 1.0;
    public string[] activeBuffs;
    public HudElementBuffs (ICoreClientAPI capi) : base(capi){
        capi.Event.RegisterGameTickListener(OnGameTick, 20);
    }

    private void OnGameTick(float dt){
        UpdateBuffs();
    }

    private void UpdateBuffs(){
        bool PoisonOak = BuffStuff.BuffManager.IsBuffActive(capi.World.Player.Entity, "PoisonOak");
        bool StingingNettle = BuffStuff.BuffManager.IsBuffActive(capi.World.Player.Entity, "StingingNettle");
        if(PoisonOak){
            if(StingingNettle){
                if(BuffStuff.BuffManager.GetActiveBuff(capi.World.Player.Entity, "PoisonOak").TickCounter > 
                   BuffStuff.BuffManager.GetActiveBuff(capi.World.Player.Entity, "StingingNettle").TickCounter){
                       activeBuffs[0] = "PoisonOak";
                       activeBuffs[1] = "StingingNettle";
                } else {
                        activeBuffs[0] = "StingingNettle";
                       activeBuffs[1] = "PoisonOak"; 
                }
            } else {
                activeBuffs[0] = "PoisonOak";
                activeBuffs[1] = "";
            }
        }
        if(StingingNettle){
            activeBuffs[0] = "StingingNettle";
            activeBuffs[1] = "";
        }
    }
    private void SetupDialog() {
        // Auto-sized dialog at the center of the screen
        ElementBounds dialogBounds = ElementStdBounds.DialogBackground().WithAlignment(EnumDialogArea.RightBottom);

        // Just a simple 300x300 pixel box
        ElementBounds textBounds = ElementBounds.Fixed(0, 0, 300, 300);

        // Background boundaries. Again, just make it fit it's child elements, then add the text as a child element
        ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
        bgBounds.BothSizing = ElementSizing.FitToChildren;
        bgBounds.WithChildren(textBounds);
        // Lastly, create the dialog
            SingleComposer = capi.Gui.CreateCompo("HudElementBuffs", dialogBounds)
            .AddShadedDialogBG(bgBounds)
            .AddStaticText(activeBuffs[0],CairoFont.WhiteDetailText(), textBounds)
            .AddStaticText(activeBuffs[1],CairoFont.WhiteDetailText(), textBounds)
            .AddStaticText("This is a piece of text at the center of your screen - Enjoy!", CairoFont.WhiteDetailText(), textBounds)
            .Compose()
        ;
        TryOpen();
    }
    public override void OnOwnPlayerDataReceived()
    {
        SetupDialog();
        UpdateBuffs();
    }
    public override void OnRenderGUI(float deltaTime)
    {
        if (capi.World.Player.WorldData.CurrentGameMode == EnumGameMode.Spectator) return;

        base.OnRenderGUI(deltaTime);
    }

    public override bool TryClose() => false;

    public override bool ShouldReceiveKeyboardEvents() => false;

    public override bool Focusable => false;
    
}}
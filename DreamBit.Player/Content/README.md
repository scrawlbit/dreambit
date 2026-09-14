# Content (shaders MGCB)

`PostProcess.fx` é o shader de pós-processamento (grayscale + tint). O `.xnb` é o efeito
compilado (DesktopGL) copiado para junto do executável pelo csproj.

Recompilar (precisa da ferramenta local `dotnet-mgcb`, já no manifesto):

    dotnet mgcb /platform:DesktopGL /build:DreamBit.Player/Content/PostProcess.fx /outputDir:out /intermediateDir:obj
    cp out/DreamBit.Player/Content/PostProcess.xnb PostProcess.xnb

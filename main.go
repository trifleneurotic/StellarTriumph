package main

import (
	"github.com/oakmound/oak/v3"
	"github.com/oakmound/oak/v3/render"
	"github.com/oakmound/oak/v3/scene"
)

func main() {
	oak.AddScene("blank", scene.Scene{
		Start: func(ctx *scene.Context) {
			ctx.DrawStack.Draw(render.NewDrawFPS(0, nil, 10, 10))
			ctx.DrawStack.Draw(render.NewLogicFPS(0, nil, 10, 20))
		},
	})
	oak.Init("blank")
}

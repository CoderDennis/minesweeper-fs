﻿module MinesweeperUI

open System
open ConsolePrinting
open Cells
open Games

let mutable debug = false

let private defaultText text = (text, ConsoleColor.Green)

let private emptyText = defaultText " "
let private mineText = ("*", ConsoleColor.Red)
let private cursorText = ("@", ConsoleColor.Green)
let private flagText = ("?", ConsoleColor.Magenta)
let private hiddenCellText = ("·", ConsoleColor.White)
let private hiddenCellDebugText = ("H", ConsoleColor.White)

let private hiddenCell cell =
    match debug, cell.IsMine with
    | true,true -> mineText
    | true, false -> hiddenCellDebugText
    | false, _ -> hiddenCellText

let private getExposedCharText cell =
    match cell.IsMine with
    | true -> mineText
    | false ->
        match cell.SurroundingCount with
        | None | Some 0 -> emptyText
        | Some i -> 
            let color = 
                match i with
                | 1 -> ConsoleColor.Cyan
                | 2 -> ConsoleColor.DarkCyan
                | 3 -> ConsoleColor.Yellow
                | 4 -> ConsoleColor.DarkRed
                | _ -> ConsoleColor.Red
            (i.ToString(), color)

let private getCellChar game cell =
    let exposedChar = lazy (getExposedCharText cell)
    match game.CursorPosition = cell.Coords with
    | true -> cursorText
    | false ->
        match (cell.State, game.State) with
        | (_, Dead) 
        | (Exposed, _) ->  exposedChar.Value
        | (Hidden, _) -> hiddenCell cell            
        | (Flagged, _) -> flagText

let private getRowText game row = 
    let inner = 
        row 
        |> List.map (getCellChar game) 
        |> List.map (fun (text, color) -> (text + " ", color))
    [("║", ConsoleColor.Green)] @ inner @ [("║\r\n", ConsoleColor.Green)]

let private getRowsText game =
    game.Cells
        |> Map.toList
        |> List.map snd
        |> List.sortBy (fun c -> c.Coords.Index)
        |> List.chunkBySize game.GameSize.Width
        |> List.map (getRowText game)

let private getHeader game left right = 
    let inside =
        [0..(game.GameSize.Width - 2)]
        |> Seq.map (fun x -> "══")
        |> String.concat ""
    left + inside + right 


let getGameDisplay game =
    let help = "Use arrow keys to move | Space to sweep | f to flag | q to quit"
    let headerTop = getHeader game "╔═" "═╗"
    let headerBottom = getHeader game "╚═" "═╝"
    let rows = getRowsText game |> List.collect id
    let stateMessage = 
        match game.State with
        | Start | Playing | Exit -> ""
        | Win -> 
            "You won!"
        | Dead -> 
            "You have exploded! :("
        | Quit -> 
            "Quitter!"

    let top = [
        defaultText "F# Minesweeper\r\n";
        defaultText help;
        defaultText "\r\n";
        defaultText headerTop;
        defaultText "\r\n";
    ]
    let bottom = [
        defaultText headerBottom;
        defaultText "\r\n\r\n";
        defaultText stateMessage;
    ]
    (top @ rows @ bottom) |> ConsoleText.withCoords ConsoleCoords.origin
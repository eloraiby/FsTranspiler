//
// F# Transpiler
// Copyright (C) 2015-2016  Wael El Oraiby
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
open System
open System.Reflection
open System.Runtime.InteropServices
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations

[<EntryPoint>]
let main argv = 
    match argv with
    | [| path |] ->
        try
            let path =
                if IO.Path.IsPathRooted path
                then path
                else IO.Path.GetFullPath path

            let asm = Assembly.LoadFile path
            0

        with e ->
            printfn "failed to load assembly %s:\n%s" path e.Message
            2

    | _ ->
        printfn "usage: %s assembly-filename" (System.Reflection.Assembly.GetExecutingAssembly().GetName().Name)
        1 // return an integer exit code


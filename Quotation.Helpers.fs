//
// F# Transpiler
// Copyright (C) 2015  Wael El Oraiby
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
module Quotation.Helpers

open System.Reflection
open System.Runtime.InteropServices
open Microsoft.FSharp.Reflection

open Reflection.Helpers

type Module = {
    Name    : string
    Type    : Type
}

type Record = {
    Name    : string
    Type    : Type
}

type Union = {
    Name    : string
    Type    : Type
}

type Tuple  = {
    Elements: Type[]
    Type    : Type
}

type Function = {
    Name    : string
    Type    : Type
}

type Struct = {
    Name    : string
    Type    : Type
}

type Class  = {
    Name    : string
    Type    : Type
}

type ExceptionRep = {
    Name    : string
    Type    : Type
}

type EnumRep = {
    Name    : string
    Type    : Type
}

let (|FSharpModule|_|) (typ: System.Type) =
    if FSharpType.IsModule typ
    then
        let typ = Type typ
        Some { Module.Name = typ.Name; Type = typ }
    else None

let (|FSharpRecord|_|) (typ: System.Type) =
    if FSharpType.IsRecord typ
    then
        let typ = Type typ      
        Some { Record.Name = typ.Name; Type = typ }
    else None

let (|FSharpUnion|_|) (typ: System.Type) =
    if FSharpType.IsUnion typ
    then
        let typ = Type typ
        Some { Union.Name = typ.Name; Type = typ }
    else None

let (|FSharpTuple|_|) (typ: System.Type) =
    if FSharpType.IsTuple typ
    then
        Some { Tuple.Elements = FSharpType.GetTupleElements typ |> Array.map(fun t ->Type(t)); Type = Type typ }
    else None

let (|FSharpFunction|_|) (typ: System.Type) =
    if FSharpType.IsFunction typ
    then
        let typ = Type typ
        Some { Function.Name = typ.Name; Type = typ }
    else None

let (|FSharpException|_|) (typ: System.Type) =
    if FSharpType.IsExceptionRepresentation typ
    then
        let typ = Type typ
        Some { ExceptionRep.Name = typ.Name; Type = typ }
    else None

let (|FSharpStruct|_|) (typ: System.Type) =
    if typ.IsValueType && not typ.IsEnum && not typ.IsPrimitive
    then
        let typ = Type typ
        Some { Struct.Name = typ.Name; Type = typ }
    else None

let (|FSharpPrimitive|_|) (typ: System.Type) =
    if typ.IsPrimitive
    then Some (Type typ)
    else None

let (|FSharpEnum|_|) (typ: System.Type) =
    if typ.IsEnum
    then
        let typ = Type typ
        Some { EnumRep.Name = typ.Name; Type = typ }
    else None

let (|FSharpClass|_|) (typ: System.Type) =
    if not typ.IsValueType
       && not typ.IsEnum
       && not typ.IsPrimitive
       && not (FSharpType.IsModule typ)
       && not (FSharpType.IsRecord typ)
       && not (FSharpType.IsUnion typ)
       && not (FSharpType.IsTuple typ)
       && not (FSharpType.IsExceptionRepresentation typ)
    then
        let typ = Type typ
        Some { Class.Name = typ.Name; Type = typ }
    else None

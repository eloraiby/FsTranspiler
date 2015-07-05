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

module AST
open System

type Module = {
    Name            : string
    Declarations    : Declaration list
}

and Struct = {
    Name            : string
    Fields          : Field list
}

and Record = {
    Name            : string
    Fields          : Field list
}

and FuncType = {
    RetType         : RichType
    Params          : RichType list
}
and AccessMode =
    | Public
    | Protected
    | Private

and Field = {
    Name            : string
    Type            : RichType
    AccessMode      : AccessMode
}

and Function = {
    Name            : string
    Type            : FuncType
}

and Union = {
    Name            : string
    Cases           : Field list
}

and Enum = {
    Name            : string
    Enums           : (string * int) list
}

and PrimitiveType =
    | Char
    | U8
    | U16
    | U32
    | U64
    | S8
    | S16
    | S32
    | S64
    | R32
    | R64
    | String

and RichType =
    | Primitive of PrimitiveType
    | Struct    of Struct
    | Record    of Record
    | FuncType  of FuncType
    | Union     of Union
    | Enum      of Enum

and Declaration =
   | Module     of Module
   | Struct     of Struct
   | Record     of Record
   | Union      of Union
   | Enum       of Enum


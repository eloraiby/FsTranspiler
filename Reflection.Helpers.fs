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

module Reflection.Helpers

open System
open System.Reflection
open System.Runtime.InteropServices
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations

let private cleanupName (s: string) =
    if String.length s <> 0
    then
        // replace @ with _
        let s = s.Replace("@", "_")
        // replace . with __
        let s = s.Replace(".", "__")
        // if it ends with `N cut it
        let trimPosition = s.IndexOf '`'
        if trimPosition <> -1
        then s.[0..trimPosition - 1]
        else s
    else s

type FieldAccess =
    | Public
    | Protected
    | Private

type TypeDeclAccess =
    | Public
    | Protected
    | Private
    | Global

type Type(orig: System.Type) =
    let rec getFullName orig =
        let mutable t = orig
        let mutable s = getName t
        while t.IsNested do
            s <- (getName t.DeclaringType) + "::" + s
            t <- t.DeclaringType
        s

    and getName (orig : System.Type) =
        if orig.IsArray
        then sprintf "Array<%s>" (getFullName (orig.GetElementType()))
        elif orig.IsGenericType
        then
            let gps =
                orig.GetGenericArguments()
                |> Array.fold
                    (fun state t ->
                        if state = ""
                        then sprintf "%s" (getFullName t)
                        else sprintf "%s, %s" state ((Type t).FullName)) ""
            sprintf "%s<%s>" (getFullName (orig.GetGenericTypeDefinition())) gps
        else sprintf "%s" (cleanupName orig.Name)

    let name    = getName orig
    let fullName = getFullName orig


    member x.Name           = name
    member x.FullName       = fullName
    member x.DeclaringType  = Type(orig.DeclaringType)
    member x.GetMethods()   = orig.GetMethods() |> Array.map (fun mi -> MethodInfo mi)
    member x.GetEnumNames() = orig.GetEnumNames()
    member x.GetFields flags    = orig.GetFields flags |> Array.map (fun f -> FieldInfo f)

    member x.DeclAccess     =
        if not (FSharpType.IsModule orig)
        then TypeDeclAccess.Global
        elif orig.IsNestedPublic
        then TypeDeclAccess.Public
        elif orig.IsNestedPrivate
        then TypeDeclAccess.Private
        elif orig.IsNestedFamily
        then TypeDeclAccess.Protected
        else failwith "unsupported type declaration access"

    member internal x.Orig  = orig

and MethodInfo(orig: System.Reflection.MethodInfo) =
    let name = cleanupName orig.Name
    
    member x.Name   = name
    member x.Reflection = Quotations.Expr.TryGetReflectedDefinition (orig :> MethodBase)
    member x.GetParameters()    = orig.GetParameters() |> Array.map(fun pi -> ParameterInfo pi)
    member x.ReturnType = Type orig.ReturnType

and ParameterInfo(orig: System.Reflection.ParameterInfo) =
    let name = cleanupName orig.Name
    
    member pi.Name = name
    member pi.ParameterType = Type orig.ParameterType

and FieldInfo(orig: System.Reflection.FieldInfo) =
    let isPublic  = orig.IsPublic || orig.Attributes.HasFlag FieldAttributes.Public
    let isPrivate = orig.Attributes.HasFlag FieldAttributes.Private
    let isProtected   = not isPrivate && not isPublic && ((orig.Attributes.HasFlag FieldAttributes.Family) || (orig.Attributes.HasFlag FieldAttributes.FamANDAssem) || (orig.Attributes.HasFlag FieldAttributes.FamORAssem))

    member fi.Name      = cleanupName orig.Name
    member fi.FieldType = Type orig.FieldType
    member fi.Access    =
        if isPublic then
            FieldAccess.Public
        elif isProtected then
            FieldAccess.Protected
        elif isPrivate then
            FieldAccess.Private
        else failwith "unsupported field accesss mode"

and MemberInfo(orig: System.Reflection.MemberInfo) =
    member mi.Name = cleanupName mi.Name

type Enum
with
    static member GetValues(t: Type) = Enum.GetValues(t.Orig)

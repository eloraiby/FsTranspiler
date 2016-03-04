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

module Reflection.Helpers

open System
open System.Reflection
open System.Runtime.InteropServices
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations

type RT =
    | Record    of PropertyInfo  []
    | Union     of UnionCaseInfo []

type ReflectedType(orig: Type) =

    let cleanupName (n: string) =
        match n.IndexOfAny [| '@'; '`' |] with
        | x when x < 0  -> n
        | x             -> n.Substring(0, x)

    let tyName = orig.Name |> cleanupName

    let isPublic (m: MethodInfo) = m.IsPublic
    let isPrivate (m: MethodInfo) = m.IsPrivate
    let isProtected (m: MethodInfo) = m.IsFamily

    let getMethods (f: MethodInfo -> bool) =
        orig.GetMethods()
        |> Seq.filter(fun mi ->
            match mi with
            | Microsoft.FSharp.Quotations.DerivedPatterns.MethodWithReflectedDefinition d -> f mi
            | _ -> false)
        |> Seq.toArray
        |> Array.map(fun mi ->
            match mi with
            | Microsoft.FSharp.Quotations.DerivedPatterns.MethodWithReflectedDefinition d -> (mi, d)
            | _ -> failwith "impossible")
    
    let pubMethods  = getMethods isPublic
    let privMethods = getMethods isPrivate
    let protMethods = getMethods isProtected

    let isPublic (p: PropertyInfo) = p.GetSetMethod(true).IsPublic
    let isPrivate (p: PropertyInfo) = p.GetSetMethod(true).IsPrivate
    let isProtected (p: PropertyInfo) = p.GetSetMethod(true).IsFamily

    let getProperties (f: PropertyInfo -> bool) =
        orig.GetProperties()
        |> Seq.filter(fun mi ->
            match mi with
            | Microsoft.FSharp.Quotations.DerivedPatterns.PropertyGetterWithReflectedDefinition d -> f mi
            | _ -> false)
        |> Seq.toArray
        |> Array.map(fun mi ->
            match mi with
            | Microsoft.FSharp.Quotations.DerivedPatterns.PropertyGetterWithReflectedDefinition d -> (mi, d)
            | _ -> failwith "impossible")
    
    let pubProps  = getProperties isPublic
    let privProps = getProperties isPrivate
    let protProps = getProperties isProtected

    let rt  =
        if FSharpType.IsRecord orig
        then Record (FSharpType.GetRecordFields orig)
        elif FSharpType.IsUnion orig
        then Union (FSharpType.GetUnionCases orig)
        else failwith "not supported"

    member x.Name   = tyName

    member x.PublicMethods      = pubMethods
    member x.ProtectedMethods   = protMethods
    member x.PrivateMethods     = privMethods

    member x.PublicProperties       = pubProps
    member x.ProtectedProperties    = protProps
    member x.PrivateProperties      = privProps


(*
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
 *)
    member internal x.Orig  = orig

type MethodInfo(orig: System.Reflection.MethodInfo) =
    member x.Reflection = Quotations.Expr.TryGetReflectedDefinition (orig :> MethodBase)
    member x.GetParameters()    = orig.GetParameters() |> Array.map(fun pi -> ParameterInfo pi)
    member x.ReturnType = Type orig.ReturnType
    member x.Orig   = orig

type ParameterInfo(orig: System.Reflection.ParameterInfo) =
    member pi.ParameterType = Type orig.ParameterType
    member pi.Orig = orig

type FieldInfo(orig: System.Reflection.FieldInfo) =
    let isPublic  = orig.IsPublic || orig.Attributes.HasFlag FieldAttributes.Public
    let isPrivate = orig.Attributes.HasFlag FieldAttributes.Private
    let isProtected   = not isPrivate && not isPublic && ((orig.Attributes.HasFlag FieldAttributes.Family) || (orig.Attributes.HasFlag FieldAttributes.FamANDAssem) || (orig.Attributes.HasFlag FieldAttributes.FamORAssem))

    member fi.FieldType = Type orig.FieldType
    member fi.Access    =
        if isPublic then
            FieldAccess.Public
        elif isProtected then
            FieldAccess.Protected
        elif isPrivate then
            FieldAccess.Private
        else failwith "unsupported field accesss mode"
    member fi.Orig = orig

type MemberInfo(orig: System.Reflection.MemberInfo) =
    member mi.Orig = orig

type Enum
with
    static member GetValues(t: Type) = Enum.GetValues(t.Orig)

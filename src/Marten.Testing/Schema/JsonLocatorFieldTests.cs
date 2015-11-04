﻿using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using FubuCore.Reflection;
using Marten.Schema;
using Marten.Testing.Documents;
using Marten.Testing.Fixtures;
using Shouldly;

namespace Marten.Testing.Schema
{
    public class JsonLocatorFieldTests
    {
        public readonly JsonLocatorField theStringField = JsonLocatorField.For<User>(x => x.FirstName);
        public readonly JsonLocatorField theNumberField = JsonLocatorField.For<User>(x => x.Age);
        public readonly JsonLocatorField theEnumField = JsonLocatorField.For<Target>(x => x.Color);

        public void member_name_is_derived()
        {
            theStringField.MemberName.ShouldBe("FirstName");
        }

        public void has_the_member_path()
        {
            theStringField.Members.Single().ShouldBeAssignableTo<PropertyInfo>()
                .Name.ShouldBe("FirstName");
        }

        public void locator_for_string()
        {
            theStringField.SqlLocator.ShouldBe("d.data -> 'FirstName'");
        }

        public void locator_for_number()
        {
            theNumberField.SqlLocator.ShouldBe("CAST(d.data -> 'Age' as integer)");
        }

        public void locator_for_enum()
        {
            theEnumField.SqlLocator.ShouldBe("CAST(d.data -> 'Color' as integer)");
        }

        public void the_lateral_join_declaration_is_null()
        {
            theStringField.LateralJoinDeclaration.ShouldBeNull();
            theNumberField.LateralJoinDeclaration.ShouldBeNull();
            theEnumField.LateralJoinDeclaration.ShouldBeNull();
        }

        public void two_deep_members_json_locator()
        {
            var inner = ReflectionHelper.GetProperty<Target>(x => x.Inner);
            var number = ReflectionHelper.GetProperty<Target>(x => x.Number);

            var twodeep = new JsonLocatorField(new MemberInfo[] {inner, number});

            twodeep.SqlLocator.ShouldBe("CAST(d.data -> 'Inner' ->> 'Number' as integer)");
        }


        public void three_deep_members_json_locator()
        {
            var inner = ReflectionHelper.GetProperty<Target>(x => x.Inner);
            var number = ReflectionHelper.GetProperty<Target>(x => x.Number);

            var deep = new JsonLocatorField(new MemberInfo[] { inner, inner, number });

            deep.SqlLocator.ShouldBe("CAST(d.data -> 'Inner' -> 'Inner' ->> 'Number' as integer)");
        }

        public void three_deep_members_json_locator_for_string_property()
        {
            var inner = ReflectionHelper.GetProperty<Target>(x => x.Inner);
            var stringProp = ReflectionHelper.GetProperty<Target>(x => x.String);

            var deep = new JsonLocatorField(new MemberInfo[] { inner, inner, stringProp });

            deep.SqlLocator.ShouldBe("d.data -> 'Inner' -> 'Inner' ->> 'String'");
        }
    }
}
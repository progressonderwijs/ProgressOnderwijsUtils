using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.DomainUnits;
using Progress.Business.Test;
using Progress.Business.Text;
using Progress.Business.Tools;
using ProgressOnderwijsUtils;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtilsTests
{
    public sealed class UserLabelsTest : TestsWithBusinessConnection
    {
        [Test]
        [PullRequestTest]
        public void UserLabelDefinitionBusinessEdit_sets_an_available_user_label()
        {
            var session = WebSession.CreateTestSession(conn, RootOrganisatie.Dummy);
            var be = CreateUserLabelDefinitionForNonExistentTable(conn, session, "TMWJ");
            PAssert.That(() => be.Field<UserLabels?>(be.fUserLabel) != null);
        }

        [Test]
        [PullRequestTest]
        public void Creating_too_many_user_label_definitions_gives_an_error()
        {
            var session = WebSession.CreateTestSession(conn, RootOrganisatie.Dummy);
            var randomHelper = RandomHelper.Insecure(576490211);

            for (var i = 0; i < UserLabelsHelper.MaxLabelCount; i++) {
                CreateUserLabelDefinitionForNonExistentTable(conn, session, randomHelper.GetStringOfLatinUpperOrLower(4).ToUpper());
            }

            var ex = Assert.Catch<GenericEditException>(() => CreateUserLabelDefinitionForNonExistentTable(conn, session, randomHelper.GetStringOfLatinLower(4)));
            PAssert.That(() => ex.Message.Contains("Te veel labels"));
        }

        static UserLabelDefinitionBusinessEdit CreateUserLabelDefinitionForNonExistentTable(BusinessConnection conn, WebSession session, string code)
        {
            var be = new UserLabelDefinitionBusinessEdit(session);
            be.ReadNieuw(conn);
            be.Values[be.fTabelName] = "sqeetarwgffbqxpw";
            be.Values[be.fCode] = code;
            be.Values[be.fDescription] = code;
            be.Save(conn);
            return be;
        }

        [Test]
        [PullRequestTest]
        public void GetVolgOnderwijsUserLabels_does_not_crash()
        {
            UserLabelsHelper.GetVolgOnderwijsUserLabels(conn, default(Id.VolgOnderwijs));
        }

        [Test]
        [PullRequestTest]
        public void AreUserLabelsAssignedToSomeVolgOnderwijs_does_not_crash()
        {
            UserLabelsHelper.AreUserLabelsAssignedToSomeVolgOnderwijs(conn, RootOrganisatie.Dummy, UserLabels.None);
        }

        [Test]
        [PullRequestTest]
        public void Members_UserLabels_always_have_at_most_one_bit_set()
        {
            var membersWithMoreThanOneBitSet = EnumHelpers.GetValues<UserLabels>()
                .Where(ul => ((int)ul & (int)ul - 1) != 0)
                .ToArray();

            PAssert.That(() => membersWithMoreThanOneBitSet.None());
        }

        [Test]
        [DataControle]
        public void All_UserLabelDefinition_TableNames_exists()
        {
            var nonExistent = SQL($@"
                    select distinct
                        uld.TableName
                    from UserLabelDefinition uld
                    where 1=1
                        and object_id(uld.TableName) is null
                ").ReadPlain<string>(conn);

            PAssert.That(() => nonExistent.None());
        }

        static UserLabels AllUserLabels(BusinessConnection conn, RootOrganisatie rootOrganisatieId, string tableName)
            => UserLabelDefinition.GetDefinitionsByRootOrgByTable(conn, rootOrganisatieId, tableName).Aggregate(UserLabels.None, (current, next) => current | next.UserLabel);

        [Test]
        [DataControle]
        public void No_VolgOnderwijs_have_undefined_labels()
        {
            var volgOnderwijsWithUndefinedLabels = EnumHelpers
                .GetValues<RootOrganisatie>()
                .SelectMany(rootOrganisatie => SQL($@"
                        select
                            vo.OnderwijsId
                        from VolgOnderwijs vo
                        where 1=1
                            and vo.Organisatie in (select obc.Child from OrganisatieBoomCache obc where obc.Parent = {rootOrganisatie})
                            and vo.UserLabels & {~AllUserLabels(conn, rootOrganisatie, "VolgOnderwijs")} <> 0
                    ").ReadPlain<Id.VolgOnderwijs>(conn)
                )
                .ToArray();

            PAssert.That(() => volgOnderwijsWithUndefinedLabels.None());
        }
    }
}

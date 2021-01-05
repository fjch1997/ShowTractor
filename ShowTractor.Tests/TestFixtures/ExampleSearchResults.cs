using ShowTractor.Plugins.Interfaces;
using System;
using System.Collections.Generic;

namespace ShowTractor.Tests.TestFixtures
{
    class ExampleSearchResults
    {
        public static TvEpisode TestEpisode1 =>
            new(
                1,
                "That Hope Is You, Part 1",
                "Arriving 930 years in the future, Burnham navigates a galaxy she no longer recognizes while searching for the rest of the U.S.S. Discovery crew.",
                null,
                null,
                new DateTime(2020, 10, 16, 0, 0, 0, DateTimeKind.Utc),
                new TimeSpan(0, 42, 11)
            );
        public static TvEpisode TestEpisode1Updated =>
            new(
                1,
                "Updated Name",
                "Updated Description",
                null,
                null,
                new DateTime(2020, 10, 17, 0, 0, 0, DateTimeKind.Utc),
                new TimeSpan(0, 42, 09)
            );
        public static TvEpisode TestEpisode2 =>
            new(
                2,
                "Far From Home",
                "The crew of the Discovery races to repair their ship after a they crash. Tilly & Saru make a first contact looking for Burnham.",
                null,
                null,
                new DateTime(2020, 10, 22, 0, 0, 0, DateTimeKind.Utc),
                new TimeSpan(0, 42, 11)
            );
        public static TvEpisode TestEpisode3 =>
            new(
                3,
                "People of Earth",
                "Reunited with Burnham, Discovery heads to Earth to find out what has happened to the Federation in the last thousand years.",
                null,
                null,
                new DateTime(2020, 10, 29, 0, 0, 0, DateTimeKind.Utc),
                new TimeSpan(0, 42, 11)
            );
        public static TvEpisode TestEpisode3RecentAirDate =>
            new(
                3,
                "People of Earth",
                "Reunited with Burnham, Discovery heads to Earth to find out what has happened to the Federation in the last thousand years.",
                null,
                null,
                DateTime.UtcNow.Date.AddDays(3),
                new TimeSpan(0, 42, 11)
            );
        public static TvEpisode TestEpisode4 =>
            new(
                4,
                "Test Episode 4",
                "Test Episode 4 description.",
                null,
                null,
                new DateTime(2020, 11, 6, 0, 0, 0, DateTimeKind.Utc),
                new TimeSpan(0, 39, 10)
            );
        public static TvSeason TestTvSeason1 =>
            new(
                null,
                "Star Trek: Discovery",
                1,
                new string[] { "Action", "Adventure" },
                new string[] { "TV-MA" },
                "Follow the voyages of Starfleet on their missions to discover new worlds and new life forms, and one Starfleet officer who must learn that to truly understand all things alien, you must first understand yourself.",
                "After a century of silence, war erupts between the Federation and Klingon Empire, with a disgraced Starfleet officer at the center of the conflict.",
                null,
                null,
                false,
                false,
                new TvEpisode[] { TestEpisode1, TestEpisode2, TestEpisode3RecentAirDate, },
            new Dictionary<string, string>()
        );
        public static TvSeason TestTvSeason1Updated =>
            TestTvSeason1 with
            {
                SeasonDescription = "Updated Description",
                Episodes = new TvEpisode[] { TestEpisode1Updated, TestEpisode2, TestEpisode3RecentAirDate, TestEpisode4 },
            };
        public static TvSeason TestTvSeason2 =>
            new(
                null,
                "Star Trek: Discovery",
                2,
                new string[] { "Action", "Adventure" },
                new string[] { "TV-MA" },
                "Follow the voyages of Starfleet on their missions to discover new worlds and new life forms, and one Starfleet officer who must learn that to truly understand all things alien, you must first understand yourself.",
                "After answering a distress signal from the U.S.S. Enterprise, season two finds the crew of the U.S.S. Discovery joining forces with Captain Christopher Pike on a new mission to investigate seven mysterious red signals and the appearance of an unknown being called the Red Angel. While the crew must work together to unravel their meaning and origin, Michael Burnham is forced to face her past with the return of her estranged brother, Spock.",
                null,
                null,
                false,
                false,
                new TvEpisode[] { TestEpisode1, TestEpisode2, TestEpisode3RecentAirDate, },
            new Dictionary<string, string>()
        );
        public static TvSeason TestTvSeason3 =>
            new(
                null,
                "Star Trek: Discovery",
                3,
                new string[] { "Action", "Adventure" },
                new string[] { "TV-MA" },
                "Follow the voyages of Starfleet on their missions to discover new worlds and new life forms, and one Starfleet officer who must learn that to truly understand all things alien, you must first understand yourself.",
                "After making the jump in the second season finale, season three finds the U.S.S. Discovery crew dropping out of the wormhole and into an unknown future far from the home they once knew. Now living in a time filled with uncertainty, the U.S.S. Discovery crew, along with the help of some new friends, must together fight to regain a hopeful future.",
                null,
                null,
                false,
                false,
                new TvEpisode[] { TestEpisode1, TestEpisode2, TestEpisode3RecentAirDate, },
            new Dictionary<string, string>()
        );
        public static TvSeason TestTvSeason4 =>
            new(
                null,
                "Shameless",
                9,
                new string[] { "Drama", "Comedy" },
                new string[] { "TV-MA" },
                "Chicagoan Frank Gallagher is the proud single dad of six smart, industrious, independent kids, who without him would be... perhaps better off. When Frank's not at the bar spending what little money they have, he's passed out on the floor. But the kids have found ways to grow up in spite of him. They may not be like any family you know, but they make no apologies for being exactly who they are.",
                "In season 9, political fervor hits the South Side, and the Gallaghers take justice into their own hands. Frank sees financial opportunity in campaigning and Fiona tries to build on the success of her apartment building.",
                null,
                null,
                true,
                false,
                new TvEpisode[] { TestEpisode1, TestEpisode2, TestEpisode3, },
            new Dictionary<string, string>()
        );
        public static TvSeason TestTvSeason5 =>
            new(
                null,
                "Shameless",
                10,
                new string[] { "Drama", "Comedy" },
                new string[] { "TV-MA" },
                "Chicagoan Frank Gallagher is the proud single dad of six smart, industrious, independent kids, who without him would be... perhaps better off. When Frank's not at the bar spending what little money they have, he's passed out on the floor. But the kids have found ways to grow up in spite of him. They may not be like any family you know, but they make no apologies for being exactly who they are.",
                "Season ten picks up six months after last season’s finale: Frank uses his leg injury to collect as many prescription drugs as possible and his exploits lead him to an old friend. Debbie rules over the Gallagher household with an iron fist. Lip navigates his relationship with Tami. And Gallavich returns as Ian and Mickey rekindle their relationship in prison as both cellmates and lovers.",
                null,
                null,
                true,
                false,
                new TvEpisode[] { TestEpisode1, TestEpisode2, TestEpisode3, },
            new Dictionary<string, string>()
        );
        public static TvSeason TestTvSeason6 =>
            new(
                null,
                "Shameless",
                11,
                new string[] { "Drama", "Comedy" },
                new string[] { "TV-MA" },
                "Chicagoan Frank Gallagher is the proud single dad of six smart, industrious, independent kids, who without him would be... perhaps better off. When Frank's not at the bar spending what little money they have, he's passed out on the floor. But the kids have found ways to grow up in spite of him. They may not be like any family you know, but they make no apologies for being exactly who they are.",
                "As Frank confronts his own mortality and family ties in his alcoholic and drug induced twilight years, Lip struggles with the prospect of becoming the family’s new patriarch. Newlyweds Ian and Mickey are figuring out the rules and responsibilities of being in a committed relationship while Deb embraces her individuality and single motherhood. Carl finds an unlikely new career in law enforcement and Kevin and V struggle to decide whether a hard life on the South Side is worth fighting for.",
                null,
                null,
                true,
                true,
                new TvEpisode[] { TestEpisode1, TestEpisode2, TestEpisode3, },
            new Dictionary<string, string>()
        );
    }
}

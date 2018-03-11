using System;
using System.Drawing;
using System.Windows.Forms;
using FluentAssertions;
using GitCommands.Settings;
using GitUI;
using NUnit.Framework;

namespace GitUITests
{
    [TestFixture]
    public class SplitterManagerTest
    {
        private MemorySettings _settings;
        private const float _designTimeFontSize = 10;
        private const int _designTimeSplitterWidth = 100;
        private const int _designTimeSplitterDistance = 40;

        [SetUp]
        public void SetupTest()
        {
            _settings = new MemorySettings();
        }

        [Test]
        public void ForNoFixedPanel_WhenWidthChanges_DistanceChangesEvenly()
        {
            // arrange
            const string splitterName = "splitterName";
            const int splitterWidth = 100;
            const int splitterDistance = 30;
            {
                SplitterManager splitManager = new SplitterManager(_settings, _designTimeFontSize);
                SplitContainer splitter = CreateVerticalSplitContainer();
                splitter.Width = splitterWidth;
                splitter.SplitterDistance = splitterDistance;
                splitManager.AddSplitter(splitter, splitterName);
                splitManager.SaveSplitters();
            }

            {
                // act
                SplitterManager splitManager = new SplitterManager(_settings, _designTimeFontSize);
                SplitContainer splitter = CreateVerticalSplitContainer();
                splitManager.AddSplitter(splitter, splitterName);
                splitter.Width = 2 * splitterWidth;
                splitManager.RestoreSplitters();

                // assert
                splitter.SplitterDistance.Should().Be(splitterDistance * 2);
            }
        }

        [TestCase(100)]
        [TestCase(-100)]
        public void ForFixedPanel1_WhenWidthChanges_DistanceDoesNotChange(int deltaWidth)
        {
            // arrange
            const string splitterName = "splitterName";
            const int splitterWidth = 200;
            const int splitterDistance = 70;
            {
                SplitterManager splitManager = new SplitterManager(_settings, _designTimeFontSize);
                SplitContainer splitter = CreateVerticalSplitContainer();
                splitter.Width = splitterWidth;
                splitter.SplitterDistance = splitterDistance;
                splitManager.AddSplitter(splitter, splitterName);
                splitManager.SaveSplitters();
            }

            {
                // act
                SplitterManager splitManager = new SplitterManager(_settings, _designTimeFontSize);
                SplitContainer splitter = CreateVerticalSplitContainer();
                splitManager.AddSplitter(splitter, splitterName);
                splitter.Width = splitterWidth + deltaWidth;
                splitter.FixedPanel = FixedPanel.Panel1;
                splitManager.RestoreSplitters();

                // assert
                splitter.SplitterDistance.Should().Be(splitterDistance);
            }
        }

        [TestCase(100)]
        [TestCase(-100)]
        public void ForFixedPanel2_WhenWidthChanges_DistanceChangesAlong(int deltaWidth)
        {
            // arrange
            const string splitterName = "splitterName";
            const int splitterWidth = 200;
            const int splitterDistance = 130;
            {
                SplitterManager splitManager = new SplitterManager(_settings, _designTimeFontSize);
                SplitContainer splitter = CreateVerticalSplitContainer();
                splitter.Width = splitterWidth;
                splitter.SplitterDistance = splitterDistance;
                splitManager.AddSplitter(splitter, splitterName);
                splitManager.SaveSplitters();
            }

            {
                // act
                SplitterManager splitManager = new SplitterManager(_settings, _designTimeFontSize);
                SplitContainer splitter = CreateVerticalSplitContainer();
                splitManager.AddSplitter(splitter, splitterName);
                int splitterNewWidth = splitterWidth + deltaWidth;
                splitter.Width = splitterNewWidth;
                splitter.FixedPanel = FixedPanel.Panel2;
                splitManager.RestoreSplitters();

                // assert splitter moved by the width delta
                splitter.SplitterDistance.Should().Be(splitterDistance + deltaWidth);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DistanceDoesNotChangeWhenGoesBelowPanel1MinSize(bool applyMinSize)
        {
            // arrange
            const string splitterName = "splitterName";
            const int splitterWidth = 200;
            const int splitterDistance = 120;
            {
                SplitterManager splitManager = new SplitterManager(_settings, _designTimeFontSize);
                SplitContainer splitter = CreateVerticalSplitContainer();
                splitter.Width = splitterWidth;
                splitter.SplitterDistance = splitterDistance;
                splitManager.AddSplitter(splitter, splitterName);
                splitManager.SaveSplitters();
            }

            {
                // act
                SplitterManager splitManager = new SplitterManager(_settings, _designTimeFontSize);
                SplitContainer splitter = CreateVerticalSplitContainer();
                splitManager.AddSplitter(splitter, splitterName);
                const int splitterNewWidth = 180;
                splitter.Width = splitterNewWidth;
                splitter.SplitterDistance = splitterDistance;
                splitter.FixedPanel = FixedPanel.Panel2;
                if (applyMinSize)
                {
                    splitter.Panel1MinSize = 110;
                }

                splitManager.RestoreSplitters();

                // assert
                if (applyMinSize)
                {
                    splitter.SplitterDistance.Should().Be(splitterDistance);
                }
                else
                {
                    splitter.SplitterDistance.Should().Be(100);
                }
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DistanceDoesNotChangeWhenGoesBelowPanel2MinSize(bool applyMinSize)
        {
            // arrange
            const string splitterName = "splitterName";
            const int splitterWidth = 200;
            const int splitterDistance = 120;
            {
                SplitterManager splitManager = new SplitterManager(_settings, _designTimeFontSize);
                SplitContainer splitter = CreateVerticalSplitContainer();
                splitter.Width = splitterWidth;
                splitter.SplitterDistance = splitterDistance;
                splitManager.AddSplitter(splitter, splitterName);
                splitManager.SaveSplitters();
            }

            {
                // act
                SplitterManager splitManager = new SplitterManager(_settings, _designTimeFontSize);
                SplitContainer splitter = CreateVerticalSplitContainer();
                splitManager.AddSplitter(splitter, splitterName);
                const int splitterNewWidth = 180;
                splitter.Width = splitterNewWidth;
                splitter.FixedPanel = FixedPanel.None;
                if (applyMinSize)
                {
                    splitter.Panel2MinSize = 110;
                }

                splitter.SplitterDistance = 60;
                splitManager.RestoreSplitters();

                // assert
                if (applyMinSize)
                {
                    splitter.SplitterDistance.Should().Be(60);
                }
                else
                {
                    splitter.SplitterDistance.Should().Be(108); // decreased by 10%
                }
            }
        }

        [TestCase(2)]
        [TestCase(-2)]
        public void ForFixedPanel1_WhenFontChanges_Panel1WidthChangesAlong(int deltaFontSize)
        {
            // arrange
            const string splitterName = "splitterName";
            const int splitterWidth = 200;
            const int splitterDistance = 70;
            {
                SplitterManager splitManager = new SplitterManager(_settings, _designTimeFontSize);
                SplitContainer splitter = CreateVerticalSplitContainer();
                splitter.Width = splitterWidth;
                splitter.SplitterDistance = splitterDistance;
                splitManager.AddSplitter(splitter, splitterName);
                splitManager.SaveSplitters();
            }

            {
                // act
                SplitterManager splitManager = new SplitterManager(_settings, _designTimeFontSize);
                SplitContainer splitter = CreateVerticalSplitContainer();
                splitManager.AddSplitter(splitter, splitterName);
                splitter.Width = splitterWidth;
                splitter.Font = new Font(splitter.Font.FontFamily, _designTimeFontSize + deltaFontSize);
                splitter.FixedPanel = FixedPanel.Panel1;
                splitManager.RestoreSplitters();

                // assert
                float scaleFactor = 1F * (_designTimeFontSize + deltaFontSize) / _designTimeFontSize;
                int expectedPanel1Width = Convert.ToInt32(splitterDistance * scaleFactor);
                splitter.SplitterDistance.Should().Be(expectedPanel1Width);
            }
        }

        [TestCase(2)]
        [TestCase(-2)]
        public void ForFixedPanel2_WhenFontChanges_Panel2WidthChangesAlong(int deltaFontSize)
        {
            // arrange
            const string splitterName = "splitterName";
            const int splitterWidth = 200;
            const int splitterDistance = 70;
            int panel2Width = splitterWidth - splitterDistance;
            {
                SplitterManager splitManager = new SplitterManager(_settings, _designTimeFontSize);
                SplitContainer splitter = CreateVerticalSplitContainer();
                splitter.Width = splitterWidth;
                splitter.SplitterDistance = splitterDistance;
                splitManager.AddSplitter(splitter, splitterName);
                splitManager.SaveSplitters();
            }

            {
                // act
                SplitterManager splitManager = new SplitterManager(_settings, _designTimeFontSize);
                SplitContainer splitter = CreateVerticalSplitContainer();
                splitManager.AddSplitter(splitter, splitterName);
                splitter.Width = splitterWidth;
                splitter.Font = new Font(splitter.Font.FontFamily, _designTimeFontSize + deltaFontSize);
                splitter.FixedPanel = FixedPanel.Panel2;
                splitManager.RestoreSplitters();

                // assert
                float scaleFactor = 1F * (_designTimeFontSize + deltaFontSize) / _designTimeFontSize;
                int expectedPanel2Width = Convert.ToInt32(panel2Width * scaleFactor);
                int newPanel2Width = splitter.Width - splitter.SplitterDistance;
                newPanel2Width.Should().Be(expectedPanel2Width);
            }
        }

        [TestCase(2)]
        [TestCase(-2)]
        public void ForNoFixedPanel_WhenFontChanges_DistanceDoesNotChange(int deltaFontSize)
        {
            // arrange
            const string splitterName = "splitterName";
            const int splitterWidth = 200;
            const int splitterDistance = 70;
            {
                SplitterManager splitManager = new SplitterManager(_settings, _designTimeFontSize);
                SplitContainer splitter = CreateVerticalSplitContainer();
                splitter.Width = splitterWidth;
                splitter.SplitterDistance = splitterDistance;
                splitManager.AddSplitter(splitter, splitterName);
                splitManager.SaveSplitters();
            }

            {
                // act
                SplitterManager splitManager = new SplitterManager(_settings, _designTimeFontSize);
                SplitContainer splitter = CreateVerticalSplitContainer();
                splitManager.AddSplitter(splitter, splitterName);
                splitter.Width = splitterWidth;
                splitter.Font = new Font(splitter.Font.FontFamily, _designTimeFontSize + deltaFontSize);
                splitter.FixedPanel = FixedPanel.None;
                splitManager.RestoreSplitters();

                // assert
                splitter.SplitterDistance.Should().Be(splitterDistance);
            }
        }

        private SplitContainer CreateVerticalSplitContainer()
        {
            SplitContainer splitter = new SplitContainer();
            splitter.FixedPanel = FixedPanel.None;
            splitter.Font = new Font(splitter.Font.FontFamily, _designTimeFontSize);
            splitter.Orientation = Orientation.Vertical;
            splitter.Width = _designTimeSplitterWidth;
            splitter.SplitterDistance = _designTimeSplitterDistance;

            return splitter;
        }
    }
}

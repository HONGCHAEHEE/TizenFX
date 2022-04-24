﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Tizen.NUI.Components.Devel.Tests
{
    using tlog = Tizen.Log;

    [TestFixture]
    [Description("Controls/FlexibleView/FlexibleView.Helper")]
    public class FlexibleViewHelperTest
    {
        private const string tag = "NUITEST";

        private Vector2 scrnSize;
        private ListBridge adapter;
        private FlexibleView horizontalFlexibleView;
        private LinearLayoutManager horizontalLayoutManager;

        [SetUp]
        public void Init()
        {
            tlog.Info(tag, "Init() is called!");
            scrnSize = new Vector2(1920, 1080);
        }

        [TearDown]
        public void Destroy()
        {
            scrnSize?.Dispose();
            tlog.Info(tag, "Destroy() is called!");
        }

        private FlexibleView GetHorizontalFlexibleView()
        {
            horizontalFlexibleView = new FlexibleView();
            Assert.IsNotNull(horizontalFlexibleView, "should be not null");
            Assert.IsInstanceOf<FlexibleView>(horizontalFlexibleView, "should be an instance of testing target class!");

            horizontalFlexibleView.Name = "FlexibleView";
            horizontalFlexibleView.WidthSpecification = 400;
            horizontalFlexibleView.HeightSpecification = 450;
            horizontalFlexibleView.Padding = new Extents(10, 10, 10, 10);
            horizontalFlexibleView.BackgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.4f);

            List<ListItemData> dataList = new List<ListItemData>();
            for (int i = 0; i < 4; ++i)
            {
                dataList.Add(new ListItemData(i));
            }
            adapter = new ListBridge(dataList);
            horizontalFlexibleView.SetAdapter(adapter);
            horizontalFlexibleView.OnRelayout(scrnSize, null);

            horizontalLayoutManager = new LinearLayoutManager(LinearLayoutManager.HORIZONTAL);
            horizontalFlexibleView.SetLayoutManager(horizontalLayoutManager);
            horizontalFlexibleView.OnRelayout(scrnSize, null);

            return horizontalFlexibleView;
        }

        [Test]
        [Category("P1")]
        [Description("ChildHelper RemoveViewAt.")]
        [Property("SPEC", "Tizen.NUI.Components.FlexibleView.ChildHelper.RemoveViewAt M")]
        [Property("SPEC_URL", "-")]
        [Property("CRITERIA", "MR")]
        [Property("COVPARAM", "")]
        [Property("AUTHOR", "guowei.wang@samsung.com")]
        public void ChildHelperRemoveViewAt()
        {
            tlog.Debug(tag, $"ChildHelperRemoveViewAt START");

            var testingTarget = GetHorizontalFlexibleView();
            Assert.IsNotNull(testingTarget, "should be not null");
            Assert.IsInstanceOf<FlexibleView>(testingTarget, "should be an instance of testing target class!");

            try
            {
                testingTarget.GetChildHelper().RemoveViewAt(1);
            }
            catch (Exception e)
            {
                tlog.Debug(tag, e.Message.ToString());
                Assert.Fail("Caught Exception : Failed!");
            }

            testingTarget.Dispose();
            tlog.Debug(tag, $"ChildHelperRemoveViewAt END (OK)");
        }

        [Test]
        [Category("P1")]
        [Description("ChildHelper RemoveViewsRange.")]
        [Property("SPEC", "Tizen.NUI.Components.FlexibleView.ChildHelper.RemoveViewsRange M")]
        [Property("SPEC_URL", "-")]
        [Property("CRITERIA", "MR")]
        [Property("COVPARAM", "")]
        [Property("AUTHOR", "guowei.wang@samsung.com")]
        public void ChildHelperRemoveViewsRange()
        {
            tlog.Debug(tag, $"ChildHelperRemoveViewsRange START");

            var testingTarget = GetHorizontalFlexibleView();
            Assert.IsNotNull(testingTarget, "should be not null");
            Assert.IsInstanceOf<FlexibleView>(testingTarget, "should be an instance of testing target class!");

            try
            {
                testingTarget.GetChildHelper().RemoveViewsRange(1, 2);
            }
            catch (Exception e)
            {
                tlog.Debug(tag, e.Message.ToString());
                Assert.Fail("Caught Exception : Failed!");
            }

            testingTarget.Dispose();
            tlog.Debug(tag, $"ChildHelperRemoveViewsRange END (OK)");
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Dashboard.Components.Dialogs;
using Aspire.Dashboard.Components.Tests.Shared;
using Aspire.Dashboard.Model;
using Aspire.Dashboard.Model.Otlp;
using Bunit;
using Microsoft.FluentUI.AspNetCore.Components;
using Xunit;

namespace Aspire.Dashboard.Components.Tests.Dialogs;

public class FilterDialogTests : DashboardTestContext
{
    [Fact]
    public void Render_DurationFilter_UsesNumericInputAndNumericConditions()
    {
        SetupFilterDialogServices();

        var cut = RenderComponent<FilterDialog>(builder =>
        {
            builder.Add(p => p.Content, CreateContent(new FieldTelemetryFilter
            {
                Field = KnownTraceFields.DurationField,
                Condition = FilterCondition.GreaterThanOrEqual,
                Value = "50"
            }));
        });

        Assert.Single(cut.FindComponents<FluentNumberField<double?>>());
        Assert.DoesNotContain("fluent-combobox", cut.Markup);

        var conditionSelect = Assert.Single(cut.FindComponents<FluentSelect<SelectViewModel<FilterCondition>>>());
        Assert.Collection(conditionSelect.Instance.Items!,
            item => Assert.Equal(FilterCondition.Equals, item.Id),
            item => Assert.Equal(FilterCondition.NotEqual, item.Id),
            item => Assert.Equal(FilterCondition.GreaterThanOrEqual, item.Id),
            item => Assert.Equal(FilterCondition.GreaterThan, item.Id),
            item => Assert.Equal(FilterCondition.LessThanOrEqual, item.Id),
            item => Assert.Equal(FilterCondition.LessThan, item.Id));
    }

    [Fact]
    public void Render_StringFilter_UsesComboboxAndStringConditions()
    {
        SetupFilterDialogServices();

        var cut = RenderComponent<FilterDialog>(builder =>
        {
            builder.Add(p => p.Content, CreateContent(new FieldTelemetryFilter
            {
                Field = KnownTraceFields.NameField,
                Condition = FilterCondition.Contains,
                Value = "request"
            }));
        });

        Assert.Empty(cut.FindComponents<FluentNumberField<double?>>());
        Assert.Contains("fluent-combobox", cut.Markup);

        var conditionSelect = Assert.Single(cut.FindComponents<FluentSelect<SelectViewModel<FilterCondition>>>());
        Assert.Collection(conditionSelect.Instance.Items!,
            item => Assert.Equal(FilterCondition.Equals, item.Id),
            item => Assert.Equal(FilterCondition.Contains, item.Id),
            item => Assert.Equal(FilterCondition.NotEqual, item.Id),
            item => Assert.Equal(FilterCondition.NotContains, item.Id));
    }

    [Fact]
    public void Render_FilterDialog_SelectElementsNotNestedInsideLabels()
    {
        // Regression test for https://github.com/microsoft/aspire/issues/17691
        // Verifies that <fluent-select> elements in the filter dialog can receive
        // click events without being blocked by overlaying <label> elements.
        // Before the fix, labels intercepted pointer events destined for the selects,
        // causing automation tools (Playwright, Selenium) to time out on click actions.
        SetupFilterDialogServices();

        var cut = RenderComponent<FilterDialog>(builder =>
        {
            builder.Add(p => p.Content, CreateContent(new FieldTelemetryFilter
            {
                Field = KnownTraceFields.NameField,
                Condition = FilterCondition.Contains,
                Value = "test"
            }));
        });

        // Verify that fluent-select elements can be found and clicked directly.
        // If a label were overlaying the select (wrapping it as a parent), the
        // select would not be a direct child of .input-container.
        var inputContainers = cut.FindAll(".input-container");
        Assert.NotEmpty(inputContainers);

        foreach (var container in inputContainers)
        {
            var selects = container.QuerySelectorAll("fluent-select");
            foreach (var select in selects)
            {
                // The select must not be nested inside a label element.
                // This ensures click events reach the select without label interception,
                // because wrapping a web component in a <label> causes the label to
                // intercept pointer events instead of the component's shadow DOM.
                Assert.NotEqual("LABEL", select.ParentElement!.NodeName);
            }

            // Labels must not contain fluent-select elements as children.
            var labels = container.QuerySelectorAll("label");
            foreach (var label in labels)
            {
                var nestedSelects = label.QuerySelectorAll("fluent-select");
                Assert.Empty(nestedSelects);
            }
        }
    }

    private void SetupFilterDialogServices()
    {
        FluentUISetupHelpers.AddCommonDashboardServices(this);
        FluentUISetupHelpers.SetupFluentUIComponents(this);
        FluentUISetupHelpers.SetupFluentInputLabel(this);
        FluentUISetupHelpers.SetupFluentTextField(this);
        FluentUISetupHelpers.SetupFluentButton(this);
        FluentUISetupHelpers.SetupFluentList(this);
        FluentUISetupHelpers.SetupFluentCombobox(this);
    }

    private static FilterDialogViewModel CreateContent(FieldTelemetryFilter filter)
    {
        return new FilterDialogViewModel
        {
            Filter = filter,
            KnownKeys = [KnownTraceFields.NameField, KnownTraceFields.DurationField],
            PropertyKeys = [],
            GetFieldValues = field => field == KnownTraceFields.NameField
                ? new Dictionary<string, int> { ["request"] = 1 }
                : []
        };
    }
}

using System.Threading;
using System.Threading.Tasks;
using Elsa.Mediator.Contracts;
using Elsa.Modules.Scheduling.Contracts;
using Elsa.Runtime.Models;
using Elsa.Runtime.Notifications;

namespace Elsa.Modules.Scheduling.Handlers;

// Updates scheduled jobs based on the updated workflow triggers.
public class ScheduleWorkflows : INotificationHandler<WorkflowTriggersIndexed>, INotificationHandler<WorkflowExecuted>
{
    private readonly IWorkflowTriggerScheduler _workflowTriggerScheduler;
    private readonly IWorkflowBookmarkScheduler _workflowBookmarkScheduler;

    public ScheduleWorkflows(IWorkflowTriggerScheduler workflowTriggerScheduler, IWorkflowBookmarkScheduler workflowBookmarkScheduler)
    {
        _workflowTriggerScheduler = workflowTriggerScheduler;
        _workflowBookmarkScheduler = workflowBookmarkScheduler;
    }

    public async Task HandleAsync(WorkflowTriggersIndexed notification, CancellationToken cancellationToken)
    {
        await _workflowTriggerScheduler.UnscheduleTriggersAsync(notification.IndexedWorkflow.RemovedTriggers, cancellationToken);
        await _workflowTriggerScheduler.ScheduleTriggersAsync(notification.IndexedWorkflow.AddedTriggers, cancellationToken);
    }

    public async Task HandleAsync(WorkflowExecuted notification, CancellationToken cancellationToken)
    {
        var workflowExecutionContext = notification.WorkflowExecutionContext;
        var workflowInstanceId = workflowExecutionContext.Id;
        var bookmarks = workflowExecutionContext.Bookmarks;
        await _workflowBookmarkScheduler.ScheduleBookmarksAsync(workflowInstanceId, bookmarks, cancellationToken);
    }
}
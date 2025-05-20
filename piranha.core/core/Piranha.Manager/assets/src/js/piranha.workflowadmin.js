/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

piranha.workflowadmin = new Vue({
    el: "#workflowadmin",
    data: {
        loading: true,
        activeTab: "workflows",
        workflows: [],
        roleAssignments: [],
        contentItems: [],
        users: [],
        workflowRoles: ["Admin", "Editor", "LegalReviewer"],
        allStates: [],
        selectedStateFilter: "",
        selectedWorkflowHistory: null,
        newRoleAssignment: {
            username: "",
            role: ""
        }
    },
    methods: {
        load: function() {
            this.loading = true;

            fetch(piranha.baseUrl + "manager/api/workflow/definitions")
                .then(response => response.json())
                .then(result => {
                    this.workflows = result;
                    
                    // Extract all unique states for filtering
                    this.allStates = [];
                    result.forEach(workflow => {
                        workflow.states.forEach(state => {
                            if (!this.allStates.find(s => s.id === state.currentStateId)) {
                                this.allStates.push({
                                    id: state.currentStateId,
                                    name: state.currentStateName
                                });
                            }
                        });
                    });
                    
                    this.loading = false;
                    
                    this.loadRoleAssignments();
                    this.loadContentItems();
                    this.loadUsers();
                })
                .catch(error => {
                    console.log("error:", error);
                    this.loading = false;
                });
        },
        
        loadRoleAssignments: function() {
            fetch(piranha.baseUrl + "manager/api/workflow/roleassignments")
                .then(response => response.json())
                .then(result => {
                    this.roleAssignments = result;
                })
                .catch(error => {
                    console.log("error:", error);
                });
        },
        
        loadContentItems: function() {
            fetch(piranha.baseUrl + "manager/api/workflow/content")
                .then(response => response.json())
                .then(result => {
                    this.contentItems = result;
                })
                .catch(error => {
                    console.log("error:", error);
                });
        },
        
        loadContentByState: function() {
            let url = piranha.baseUrl + "manager/api/workflow/content";
            
            if (this.selectedStateFilter) {
                url += "?stateId=" + this.selectedStateFilter;
            }
            
            fetch(url)
                .then(response => response.json())
                .then(result => {
                    this.contentItems = result;
                })
                .catch(error => {
                    console.log("error:", error);
                });
        },
        
        loadUsers: function() {
            fetch(piranha.baseUrl + "manager/api/users")
                .then(response => response.json())
                .then(result => {
                    this.users = result;
                })
                .catch(error => {
                    console.log("error:", error);
                });
        },
        
        showAddRoleModal: function() {
            this.newRoleAssignment = {
                username: "",
                role: ""
            };
            $("#addRoleModal").modal("show");
        },
        
        addRoleAssignment: function() {
            if (!this.newRoleAssignment.username || !this.newRoleAssignment.role) {
                piranha.notifications.error("Please select both user and role");
                return;
            }
            
            fetch(piranha.baseUrl + "manager/api/workflow/roleassignments", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(this.newRoleAssignment)
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error("Failed to add role assignment");
                    }
                    return response.json();
                })
                .then(() => {
                    piranha.notifications.success("Role assignment added successfully");
                    $("#addRoleModal").modal("hide");
                    this.loadRoleAssignments();
                })
                .catch(error => {
                    piranha.notifications.error("Failed to add role assignment: " + error.message);
                });
        },
        
        removeRoleAssignment: function(id) {
            if (confirm("Are you sure you want to remove this role assignment?")) {
                fetch(piranha.baseUrl + "manager/api/workflow/roleassignments/" + id, {
                    method: "DELETE"
                })
                    .then(response => {
                        if (!response.ok) {
                            throw new Error("Failed to remove role assignment");
                        }
                        piranha.notifications.success("Role assignment removed successfully");
                        this.loadRoleAssignments();
                    })
                    .catch(error => {
                        piranha.notifications.error("Failed to remove role assignment: " + error.message);
                    });
            }
        },
        
        viewWorkflowHistory: function(contentId) {
            fetch(piranha.baseUrl + "manager/api/workflow/state/" + contentId)
                .then(response => response.json())
                .then(result => {
                    this.selectedWorkflowHistory = result;
                    $("#historyModal").modal("show");
                })
                .catch(error => {
                    piranha.notifications.error("Failed to load workflow history: " + error.message);
                });
        },
        
        formatDate: function(date) {
            if (!date) return "";
            return new Date(date).toLocaleString();
        },
        
        getStateBadgeClass: function(stateId) {
            switch (stateId) {
                case "draft":
                    return "badge-secondary";
                case "review":
                    return "badge-info";
                case "legal_review":
                    return "badge-warning";
                case "approved":
                    return "badge-primary";
                case "published":
                    return "badge-success";
                case "archived":
                    return "badge-dark";
                default:
                    return "badge-light";
            }
        }
    }
});

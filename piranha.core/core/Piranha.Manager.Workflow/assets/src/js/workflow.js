/*global
    piranha
*/

piranha.workflow = {
    /**
     * Gets all available workflow definitions.
     * @param {function} cb The callback
     */
    getDefinitions: function (cb) {
        fetch(`${piranha.baseUrl}/manager/api/workflow/definitions`)
            .then(function (response) { return response.json(); })
            .then(function (result) {
                if (cb)
                    cb(result);
            })
            .catch(function (error) { console.log("error:", error); });
    },

    /**
     * Gets the current workflow for the specified content item.
     * @param {string} contentId The content id
     * @param {function} cb The callback
     */
    getContentWorkflow: function (contentId, cb) {
        fetch(`${piranha.baseUrl}/manager/api/workflow/content/${contentId}`)
            .then(function (response) { return response.json(); })
            .then(function (result) {
                if (cb)
                    cb(result);
            })
            .catch(function (error) { console.log("error:", error); });
    },

    /**
     * Gets the available transitions for the specified content item.
     * @param {string} contentId The content id
     * @param {function} cb The callback
     */
    getAvailableTransitions: function (contentId, cb) {
        fetch(`${piranha.baseUrl}/manager/api/workflow/content/${contentId}/transitions`)
            .then(function (response) { return response.json(); })
            .then(function (result) {
                if (cb)
                    cb(result);
            })
            .catch(function (error) { console.log("error:", error); });
    },

    /**
     * Performs a workflow transition for the specified content item.
     * @param {string} contentId The content id
     * @param {object} model The transition model
     * @param {function} cb The callback
     */
    performTransition: function (contentId, model, cb) {
        fetch(`${piranha.baseUrl}/manager/api/workflow/content/${contentId}/transition`, {
            method: "post",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(model)
        })
            .then(function (response) { return response.json(); })
            .then(function (result) {
                if (cb)
                    cb(result);
            })
            .catch(function (error) { console.log("error:", error); });
    },

    /**
     * Creates a new workflow instance for the specified content item.
     * @param {string} contentId The content id
     * @param {object} model The creation model
     * @param {function} cb The callback
     */
    createWorkflowInstance: function (contentId, model, cb) {
        fetch(`${piranha.baseUrl}/manager/api/workflow/content/${contentId}/create`, {
            method: "post",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(model)
        })
            .then(function (response) { return response.json(); })
            .then(function (result) {
                if (cb)
                    cb(result);
            })
            .catch(function (error) { console.log("error:", error); });
    }
}; 
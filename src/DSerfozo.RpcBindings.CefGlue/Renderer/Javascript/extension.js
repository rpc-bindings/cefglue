var binding;

if (!binding) {
    binding = {};
}

(function () {
    function createPromise() {
        const promiseData = {};
        const promise = new Promise(function (resolve, reject) {
            promiseData.resolve = resolve;
            promiseData.reject = reject;
        });
        promiseData.promise = promise;
        return promiseData;
    }

    native function setPromiseInteractions();
    native function bindingRequire();
    native function promiseDone();

    binding.require = bindingRequire;

    setPromiseInteractions(createPromise, function (obj) {
        if (obj && obj.then && obj.catch) {
            return true;
        }
        return false;
    }, function (promise, id) {
        promise.then(function (result) {
            promiseDone(id, true, result, null);
        }, function (error) {
            promiseDone(id, false, null, error);
        });
    });
})();
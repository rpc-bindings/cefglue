var test;
if (!test) {
    test = {};
}

(function () {
    native function success();
    native function fail();
    test.run = async function (testFunc) {
        if (window) {
            window.onerror = function () {
                fail('Javascript error.')
            }
        }

        if (typeof testFunc === 'function') {
            try {
                const result = testFunc();
                if (result && result.then) {
                    await result;
                }
                success();
            } catch (e) {
                fail(e.toString());
            }
        } else {
            fail('Test does not exists.');
        }
    }
})();
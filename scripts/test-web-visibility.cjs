const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

// Exercise the plugin with a mock document; actual browser checks are separate.
const source = fs.readFileSync(path.join(__dirname,
    '../Packages/jp.ac.keio.sfc.sdp/Runtime/Plugins/WebGL/GcVisibility.jslib'), 'utf8')
    .replace("{{{ makeDynCall('vii', 'callback') }}}", 'callback');
const handlers = new Set();
const document = {
    hidden: false,
    addEventListener(name, handler) { assert.equal(name, 'visibilitychange'); handlers.add(handler); },
    removeEventListener(name, handler) { assert.equal(name, 'visibilitychange'); handlers.delete(handler); }
};
const context = { document, LibraryManager: { library: {} }, mergeInto: Object.assign };
vm.createContext(context);
vm.runInContext(source, context);
const lib = context.LibraryManager.library;
context.GcVisibility = lib.$GcVisibility;
const calls = [];
const callback = (id, hidden) => calls.push([id, hidden]);
assert.equal(lib.GcVisibilityRegister(1, callback), 0);
assert.equal(lib.GcVisibilityRegister(2, callback), 0);
assert.equal(handlers.size, 1);
document.hidden = true;
for (const handler of handlers) handler();
assert.deepEqual(calls, [[1, 1], [2, 1]]);
assert.equal(lib.GcVisibilityRegister(3, callback), 1);
lib.GcVisibilityUnregister(1);
lib.GcVisibilityUnregister(1);
calls.length = 0;
document.hidden = false;
for (const handler of handlers) handler();
assert.deepEqual(calls, [[2, 0], [3, 0]]);
lib.GcVisibilityUnregister(2);
lib.GcVisibilityUnregister(3);
assert.equal(handlers.size, 0);
lib.GcVisibilityRegister(4, (id, hidden) => {
    callback(id, hidden);
    lib.GcVisibilityUnregister(5);
});
lib.GcVisibilityRegister(5, callback);
calls.length = 0;
for (const handler of handlers) handler();
assert.deepEqual(calls, [[4, 0]]);
lib.GcVisibilityUnregister(4);
assert.equal(handlers.size, 0);
console.log('Web visibility plugin: passed');

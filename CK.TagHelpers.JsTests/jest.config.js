
module.exports = {
  rootDir: '..',
  roots: [
    '<rootDir>/CK.TagHelpers.JsTests/src',
    '<rootDir>/CK.Taghelpers/wwwroot/js'
  ],
  testMatch: ['**/?(*.)+(spec|test).[tj]s?(x)'],
  testEnvironment: 'jsdom',
  verbose: true,
  coverageProvider: 'v8',
  coverageDirectory: '<rootDir>/CK.TagHelpers.JsTests/coverage',
  coverageReporters: ['text', 'lcov', 'html'],
};

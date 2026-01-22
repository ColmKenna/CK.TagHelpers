module.exports = {
  testEnvironment: 'jsdom',
  testMatch: ['**/*.test.js'],
  moduleFileExtensions: ['js'],
  verbose: true,
  collectCoverageFrom: [
    '../CK.TagHelpers/wwwroot/js/**/*.js'
  ],
  coverageDirectory: 'coverage',
  coverageReporters: ['text', 'lcov', 'html']
};

const path = require('path');
const webpack = require('webpack');
const UglifyJsPlugin = require("uglifyjs-webpack-plugin");
const pkg = require('./package.json');

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);

    const clientBundleConfig = {
        stats: { modules: false },
        resolve: { extensions: ['.js'] },
        module: {
            rules: [
                {
                    test: /\.(png|jpg|jpeg|gif)(\?v=[0-9]\.[0-9]\.[0-9])?$/,
                    use: [{ loader: 'file-loader?name=src/img/[name].[ext]' }]
                },
                {
                    test: /\.(ttf|eot|woff|woff2|svg)(\?v=[0-9]\.[0-9]\.[0-9])?$/,
                    use: [{ loader: 'file-loader?name=css/fonts/[name].[ext]' }]
                },
                {
                    test: /\.css$/,
                    use: ['style-loader', 'css-loader']
                },
            ]
        },
        entry: {
            vendor: Object.keys(pkg.dependencies)
        },
        output: {
            publicPath: '/dist/',
            filename: '[name].js',
            library: '[name]_[hash]',
            path: path.join(__dirname, 'wwwroot', 'dist')
        },
        plugins: [
            // Plugins that apply in development and production builds
            new webpack.NormalModuleReplacementPlugin(/\/iconv-loader$/, require.resolve('node-noop')), // Workaround for https://github.com/andris9/encoding/issues/16
            new webpack.DefinePlugin({
                'process.env.NODE_ENV': isDevBuild ? '"development"' : '"production"'
            }),
            new webpack.DefinePlugin({ "global.GENTLY": false }), // WORKAROUND: webpack -> superagent uses require
            new webpack.DllPlugin({
                path: path.join(__dirname, 'wwwroot', 'dist', '[name]-manifest.json'),
                name: '[name]_[hash]'
            })
        ].concat(isDevBuild
            ? [
                // Plugins that apply in development builds only
            ]
            : [
                // Plugins that apply in production builds only
                new UglifyJsPlugin({
                    cache: true,
                    parallel: true
                })
            ]
        ),
        node: {
            __dirname: true,
        }
    };

    return clientBundleConfig;
};

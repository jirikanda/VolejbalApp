const path = require('path');
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
const UglifyJsPlugin = require("uglifyjs-webpack-plugin");
const pkg = require('./package.json');

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);
    const extractCSS = new ExtractTextPlugin({
        filename: "vendor.css",
        disable: isDevBuild
    });

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
                    test: /\.scss$(\?|$)/,
                    use: extractCSS.extract({
                        fallback: "style-loader",
                        use: ["css-loader", "sass-loader"]
                    })
                },
                {
                    test: /\.css(\?|$)/,
                    use: extractCSS.extract({
                        use: isDevBuild ? 'css-loader' : 'css-loader?minimize'
                    })
                }
            ]
        },
        entry: {
            vendor: Object.keys(pkg.dependencies).concat("font-awesome/scss/font-awesome.scss")
        },
        output: {
            publicPath: '/dist/',
            filename: '[name].js',
            library: '[name]_[hash]',
            path: path.join(__dirname, 'wwwroot', 'dist')
        },
        plugins: [
            // Plugins that apply in development and production builds
            new webpack.ProvidePlugin({ $: 'jquery', jQuery: 'jquery' }), // Maps these identifiers to the jQuery package (because Bootstrap expects it to be a global variable)
            new webpack.NormalModuleReplacementPlugin(/\/iconv-loader$/, require.resolve('node-noop')), // Workaround for https://github.com/andris9/encoding/issues/16
            new webpack.DefinePlugin({
                'process.env.NODE_ENV': isDevBuild ? '"development"' : '"production"'
            }),
            new webpack.DefinePlugin({ "global.GENTLY": false }), // WORKAROUND: webpack -> superagent uses require
            new webpack.ProvidePlugin({ // Bootstrap dependencies
                $: "jquery",
                jQuery: "jquery",
                "window.jQuery": "jquery",
                Tether: "tether",
                "window.Tether": "tether",
                Alert: "exports-loader?Alert!bootstrap/js/dist/alert",
                Button: "exports-loader?Button!bootstrap/js/dist/button",
                Carousel: "exports-loader?Carousel!bootstrap/js/dist/carousel",
                Collapse: "exports-loader?Collapse!bootstrap/js/dist/collapse",
                Dropdown: "exports-loader?Dropdown!bootstrap/js/dist/dropdown",
                Modal: "exports-loader?Modal!bootstrap/js/dist/modal",
                Popover: "exports-loader?Popover!bootstrap/js/dist/popover",
                Scrollspy: "exports-loader?Scrollspy!bootstrap/js/dist/scrollspy",
                Tab: "exports-loader?Tab!bootstrap/js/dist/tab",
                Tooltip: "exports-loader?Tooltip!bootstrap/js/dist/tooltip",
                Util: "exports-loader?Util!bootstrap/js/dist/util",
                Popper: ['popper.js', 'default']
            }),
            extractCSS,
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

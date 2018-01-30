const path = require('path');
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
const merge = require('webpack-merge');
const extractCSS = new ExtractTextPlugin({
    filename: "vendor.css",
    disable: process.env.NODE_ENV === "development"
});

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);

    const sharedConfig = {
        stats: { modules: false },
        resolve: { extensions: [ '.js' ] },
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
                }
            ]
        },
        entry: {
            vendor: [
                'jquery',
                'bootstrap',
                'bootstrap-loader',
                "font-awesome/scss/font-awesome.scss",  
                'domain-task',
                'event-source-polyfill',
                'history',
                'react',
                'reactstrap',
                'react-dom',
                'react-popper',
                'react-router-dom',
                'react-redux',
                'react-slick',
                'redux',
                'redux-thunk',
                'react-router-redux',
                'superagent',
                'superagent-prefix',
                'superagent-use',
                'superagent-defaults',
                'es6-shim',
                'redux-form',
                'http-status-codes',
                'react-redux-toastr'
            ]
        },
        output: {
            publicPath: '/dist/',
            filename: '[name].js',
            library: '[name]_[hash]',
        },
        plugins: [
            new webpack.ProvidePlugin({ $: 'jquery', jQuery: 'jquery' }), // Maps these identifiers to the jQuery package (because Bootstrap expects it to be a global variable)
            new webpack.NormalModuleReplacementPlugin(/\/iconv-loader$/, require.resolve('node-noop')), // Workaround for https://github.com/andris9/encoding/issues/16
            new webpack.DefinePlugin({
                'process.env.NODE_ENV': isDevBuild ? '"development"' : '"production"'
            }),
            new webpack.DefinePlugin({ "global.GENTLY": false }), // WORKAROUND: webpack -> superagent uses require
            new webpack.ProvidePlugin({
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
            })
        ],
        node: {
          __dirname: true,
        }
    };

    const clientBundleConfig = merge(sharedConfig, {
        output: { path: path.join(__dirname, 'wwwroot', 'dist') },
        module: {
            rules: [
                { test: /\.css(\?|$)/, use: extractCSS.extract({ use: isDevBuild ? 'css-loader' : 'css-loader?minimize' }) } 
            ]
        },
        plugins: [
            extractCSS,
            new webpack.DllPlugin({
                path: path.join(__dirname, 'wwwroot', 'dist', '[name]-manifest.json'),
                name: '[name]_[hash]'
            })
        ].concat(isDevBuild ? [] : [
            new webpack.optimize.UglifyJsPlugin()
        ])
    });

    //const serverBundleConfig = merge(sharedConfig, {
    //    target: 'node',
    //    resolve: { mainFields: ['main'] },
    //    output: {
    //        path: path.join(__dirname, 'ClientApp', 'dist'),
    //        libraryTarget: 'commonjs2',
    //    },
    //    module: {
    //        rules: [{ test: /\.css(\?|$)/, use: extractCSS.extract({ use: isDevBuild ? 'css-loader' : 'css-loader?minimize' }) } ]
    //    },
    //    entry: {
    //        vendor: ['aspnet-prerendering', 'react-dom/server']
    //    },
    //    plugins: [
    //        extractCSS,
    //        new webpack.DllPlugin({
    //            path: path.join(__dirname, 'ClientApp', 'dist', '[name]-manifest.json'),
    //            name: '[name]_[hash]'
    //        })
    //    ]
    //});

    return clientBundleConfig; //[clientBundleConfig, serverBundleConfig];
};

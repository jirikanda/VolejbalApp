const path = require("path");
const webpack = require("webpack");
const ExtractTextPlugin = require("extract-text-webpack-plugin");
const merge = require("webpack-merge");

const extractCSS = new ExtractTextPlugin({
    filename: "site.css",
    disable: process.env.NODE_ENV === "development"
});

module.exports = env => {
    const isDevBuild = !(env && env.prod);

    // Configuration in common to both client-side and server-side bundles
    const sharedConfig = () => ({
        stats: { modules: false },
        resolve: { extensions: [".js", ".jsx", ".ts", ".tsx"] },
        output: {
            filename: "[name].js",
            publicPath: "/dist/" // Webpack dev middleware, if enabled, handles requests for this URL prefix
        },
        module: {
            rules: [
                {
                    test: /\.tsx?$/,
                    include: /ClientApp/,
                    loaders: ["awesome-typescript-loader?silent=true"]
                },
                {
                    test: /\.jsx$/,
                    use: [
                        {
                            loader: "babel-loader",
                            options: { presets: ["es2017", "react"] }
                        }
                    ],
                    exclude: /node_modules/
                },
                {
                    test: /\.(png|jpg|jpeg|gif)(\?v=[0-9]\.[0-9]\.[0-9])?$/,
                    use: [
                        {
                            loader: "file-loader?name=src/img/[name].[ext]"
                        }
                    ]
                },
                {
                    test: /\.(woff2?|ttf|svg|eot)(\?v=[0-9]\.[0-9]\.[0-9])?$/,
                    use: [
                        {
                            loader: "file-loader?name=css/fonts/[name].[ext]"
                        }
                    ]
                }
            ]
        }
    });

    // Configuration for client-side bundle suitable for running in browsers
    const clientBundleOutputDir = "./wwwroot/dist";
    const clientBundleConfig = merge(sharedConfig(), {
        entry: {
            "main-client": "./ClientApp/boot-client.tsx",
            "silentRenew": "./ClientApp/oidc/silentRenew.ts"
        },
        module: {
            rules: [
                {
                    test: /\.scss$(\?|$)/,
                    use: extractCSS.extract({
                        fallback: "style-loader",
                        use: ["css-loader", "sass-loader"]
                    })
                }
            ]
        },
        output: { path: path.join(__dirname, clientBundleOutputDir) },
        plugins: [
            extractCSS,
            new webpack.DllReferencePlugin({
                context: __dirname,
                manifest: require("./wwwroot/dist/vendor-manifest.json")
            })
        ].concat(
            isDevBuild
                ? [
                    // Plugins that apply in development builds only
                    new webpack.SourceMapDevToolPlugin({
                        filename: "[file].map", // Remove this line if you prefer inline source maps
                        moduleFilenameTemplate: path.relative(
                            clientBundleOutputDir,
                            "[resourcePath]"
                        ) // Point sourcemap entries to the original file locations on disk
                    })
                ]
                : [
                    // Plugins that apply in production builds only
                    new webpack.optimize.UglifyJsPlugin()
                ]
            )
    });

    // Configuration for server-side (prerendering) bundle suitable for running in Node
    //const serverBundleConfig = merge(sharedConfig(), {
    //    resolve: { mainFields: ["main"] },
    //    entry: {
    //        "main-server": "./ClientApp/boot-server.tsx",
    //        //bootstrap: bootstrapConf
    //    },
    //    module: {
    //        rules: [
    //            {
    //                test: /\.scss$(\?|$)/,
    //                use: extractCSS.extract({
    //                    fallback: "style-loader",
    //                    use: ["css-loader", "sass-loader"]
    //                })
    //            }
    //        ]
    //    },
    //    plugins: [
    //        extractCSS,
    //        new webpack.DllReferencePlugin({
    //            context: __dirname,
    //            manifest: require("./ClientApp/dist/vendor-manifest.json"),
    //            sourceType: "commonjs2",
    //            name: "./vendor"
    //        })
    //    ],
    //    output: {
    //        libraryTarget: "commonjs",
    //        path: path.join(__dirname, "./ClientApp/dist")
    //    },
    //    target: "node",
    //    devtool: "inline-source-map"
    //});

    return clientBundleConfig;//[clientBundleConfig, serverBundleConfig];
};

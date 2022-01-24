README_ja

ユニティちゃんトゥーンシェーダー Ver.2.0

「ユニティちゃんトゥーンシェーダー」は、セル風3DCGアニメーションの制作現場での要望に応えるような形で設計された、映像志向のトゥーンシェーダーです。

ユニティちゃんトゥーンシェーダーVer.2.0では、従来の機能に加えて大幅な機能強化を行いました。
Ver.1.0でできる絵づくりをカバーしつつ、さらに高度なルックが実現できるようになっています。

【ターゲット環境】
Unity5.6.x もしくはそれ以降が必要です。
本パッケージは、Unity5.6.5p1で作成されています。

【iOS/OSX METALで使用する際の注意】
iOS/OSX METALで使用する場合、CullMode=OFF（両面表示）の時に、正しい表示が出来ない場合があります。
その場合、メッシュを両面に貼って、それぞれにCullMode=Back（背面カリング）/CullMode=Front（正面カリング）のマテリアルを設定するようにしてください。

【提供ライセンス】
「ユニティちゃんトゥーンシェーダーVer.2.0」は、UCL2.0（ユニティちゃんライセンス2.0）で提供されます。
ユニティちゃんライセンスについては、以下を参照してください。
http://unity-chan.com/contents/guideline/


【新規】
2018/02/09：2.0.4：ターゲット環境を正式にUnity5.6.x以降としました。（Unity2018.1にも対応しています）
　　　　　　　　　 コードの全面的な改修＆バグ修正の他、以下の仕様を新規に追加しました。

●Phong Tessellation対応
　対応部分のコードは、Nora氏の https://github.com/Stereoarts/UnityChanToonShaderVer2_Tess を参考にさせていただきました。
　Tessellationは、使えるプラットフォームが限られている上に、かなりパワフルなPC環境を要求しますので、覚悟して使ってください。
　想定している用途は、パワフルなWindows10/DX11のマシンを使って、映像＆VR向けに使用することです。
　Light版とあるものは、ライトをディレクショナルライト１灯に制限した代わりに軽量化したバリエーションです。
●Positionスケーリングベースのアウトラインを搭載。
　従来の法線反転方式だとアウトラインが切れてしまう、キューブのようなモデルに綺麗にアウトラインが出せます。
●クリッピング系シェーダーでアウトラインのアルファ抜きに対応。
　アルファ付きテクスチャと組み合わせた時に、背面から見てもアウトラインポリゴンがアルファに従って抜けるようにした。
●アウトライン用テクスチャ（Outline_Tex）の搭載。
●ハイカラー用テクスチャ（HiColor_Tex）の搭載。
●PostProcessingStackと一緒に使うことを前提に、エミッシブカラー（Emissive_Color）およびエミッシブ用テクスチャ（Emissive_Tex）を搭載。
　エミッシブカラー側でHDR値が設定できるので、PostProcessingStackのブルームエフェクトと組み合わせることで、エミッシブ部分を光らせることができるようになった。

　※今回搭載された新規テクスチャに関しても、今まで通り、必要なければ使わなくても問題ないようになっています。

【過去の修正履歴】
2017/06/25：2.0.3：マニュアル修正。【iOS/OSX METALで使用する際の注意】を追加。
2017/06/19：2.0.3：Set_HighColorMask、Set_RimLightMaskの追加。機能強化の結果、Set_HighColorPositionは廃止。
2017/06/09：2.0.2：Nintendo Switch、PlayStation 4に正式対応。モバイル軽量版の追加。その他機能強化。
2017/05/20：2.0.1：TransClipping系シェーダーのブレンド仕様変更とリムライトに調整機能追加。
　　　　　　　　　 上の仕様変更に伴い、トランスペアレント系シェーダーを２つ追加。
　　　　　　　　　（ToonColor_DoubleShadeWithFeather_Transparent、ToonColor_ShadingGradeMap_Transparent）
2017/05/07：2.0.0：最初のバージョン


最新バージョン：2.0.4
最終リリース日：2018.02.02
カテゴリー：3D
形式：zip


//------------------------------
README_en

Unity-Chan Toon Shader Ver.2.0

Unity-Chan Toon Shader is a toon shader for images and video that is designed to meet your needs when creating cel-shaded 3DCG animations.

We have greatly enhanced performance in Unity-Chan Toon Shader Ver. 2.0.
It still has the same rendering capabilities as Ver. 1.0, but you can now create an even more sophisticated look.

【Target Environment】
Require Unity 5.6.x or later.
※As a result responding to Nintendo Switch platform, if using with Unity 5.5.x, some errors(warnings) appear in the shader, but there is no problem in operation.

New!【Using with iOS/OSX METAL】
When using with iOS / OSX METAL, correct display may not be possible when CullMode = OFF (double-sided drawing).
In that case, make the meshes on both sides and set materials of CullMode = Back (back-face culling) / CullMode = Front (front-face culling) for each.

【License】
Unity-Chan Toon Shader 2.0 is provided under the Unity-Chan License 2.0 terms.
Please refer to the following link for information regarding the Unity-Chan License.
http://unity-chan.com/contents/guideline/

Latest Version: 2.0.3
Update: 2017.06.25
Category: 3D
File format: zip

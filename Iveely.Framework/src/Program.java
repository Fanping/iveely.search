
import com.iveely.framework.segment.WordBreaker;
import com.iveely.framework.text.syntactic.SyntacticAnalysis;
import edu.stanford.nlp.ling.CoreLabel;
import edu.stanford.nlp.ling.Sentence;
import edu.stanford.nlp.parser.lexparser.LexicalizedParser;
import edu.stanford.nlp.trees.Tree;
import java.util.List;

/**
 *
 * @author liufanping@iveely.com
 * @date 2014-12-28 21:02:35
 */
public class Program {

    public static void main(String[] args) {
        //String text = com.iveely.framework.file.Reader.readToString(new File("test case\\test6.txt"), "utf-8");
        //SyntacticAnalysis.getInstance().parse(text, false);
        // showAll();
        testSyn();
    }

    private static void testSyn() {
//        SyntacticAnalysis.getInstance().parse("", "张亚勤博士是前微软中国ceo", false, SyntacticAnalysis.Type.ALL);
//        SyntacticAnalysis.getInstance().parse("", "张三（1900年1月1日-2000年1月1日）", false, SyntacticAnalysis.Type.ALL);
//        SyntacticAnalysis.getInstance().parse("", "诺基亚公司是总部位于芬兰的通信公司", false, SyntacticAnalysis.Type.ALL);
//        SyntacticAnalysis.getInstance().parse("", "复兴航空是总部位于中国台湾的一家航空公司", false, SyntacticAnalysis.Type.ALL);
//        SyntacticAnalysis.getInstance().parse("", "复兴航空（英语译名：TransAsia Airways；IATA代码：GE，ICAO代码：TNA，呼号：TransAsia）是总部位于中国台湾的一家航空公司，主要经营台湾岛内航线以及短程国际航线。", false, SyntacticAnalysis.Type.ALL);
//        SyntacticAnalysis.getInstance().parse("少林寺", "少林寺，是位于中华人民共和国河南省登封市嵩山五乳峰下的一座佛寺，由于其座落于嵩山腹地少室山的茂密丛林之中，故名“少林寺”。", false, SyntacticAnalysis.Type.ALL);
        SyntacticAnalysis.getInstance().parse("", "英国电影学院奖创建于1947年，原主要表彰对象是英国电影及由英国籍演员演出的外国影片，相当于美国的奥斯卡奖，但近年来提名较开放，只要在英国正式上映的影片都可获提名，奖项改为面向世界各国的影片进行评奖，使之产生了更大的影响。", true, SyntacticAnalysis.Type.ALL);
    }

    private static void showAll() {
        LexicalizedParser lp = LexicalizedParser.loadModel("resources/lexparser/chineseFactored.ser.gz");
        String[] content = WordBreaker.getInstance().splitToArray("复兴航空是总部位于中国台湾的一家航空公司");
        List<CoreLabel> rawWords = Sentence.toCoreLabelList(content);
        Tree parse = lp.apply(rawWords);
        parse.pennPrint();
        System.out.println();

//        TreebankLanguagePack tlp = new PennTreebankLanguagePack();
//        GrammaticalStructureFactory gsf = tlp.grammaticalStructureFactory();
//        GrammaticalStructure gs = gsf.newGrammaticalStructure(parse);
//        List<TypedDependency> tdl = gs.typedDependenciesCCprocessed();
//        System.out.println(tdl);
//        System.out.println();
//        TreePrint tp = new TreePrint("penn,typedDependenciesCollapsed");
//        tp.printTree(parse);
    }

}
